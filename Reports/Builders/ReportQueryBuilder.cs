using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Linq.Expressions;

public class ReportQueryBuilder
{
    private readonly UnitOfWork _uow;
    public ReportQueryBuilder(UnitOfWork uow) => _uow = uow;

    public ReportResult Build(ReportConfig config)
    {
        var result = new ReportResult();
        var query = BuildQuery(config);

        if (config.ShowTotalCount)
            result.TotalCount = query.Count();

        result.Rows = config.GroupBy switch
        {
            ReportGroupBy.Author => GroupBy(query, config, t => t.Author != null ? t.Author.Name : "Без автора"),
            ReportGroupBy.State => GroupBy(query, config, t => t.State != null ? t.State.Name : "Не указано"),
            ReportGroupBy.Category => GroupBy(query, config, t => t.Category != null ? t.Category.Name : "Не указано"),
            ReportGroupBy.Priority => GroupBy(query, config, t => t.Preorety != null ? t.Preorety.Name : "Не указано"),
            ReportGroupBy.User => GroupBy(query, config, t => t.User != null ? t.User.Name : "Не указано"),
            ReportGroupBy.Company => GroupBy(query, config, t => t.Company != null ? t.Company.Name : "Не указано"),
            ReportGroupBy.Date => GroupByMonth(query, config),
            _ => GetFlatList(query)
        };

        return result;
    }

    private IQueryable<Tiket> BuildQuery(ReportConfig config)
    {
        IQueryable<Tiket> query = _uow.Query<Tiket>();   // IQueryable<Tiket>, трансляция в SQL

        if (config.DateFrom.HasValue)
            query = query.Where(t => t.DataCreted >= config.DateFrom.Value);
        if (config.DateTo.HasValue)
            query = query.Where(t => t.DataCreted <= config.DateTo.Value.Date.AddDays(1).AddTicks(-1));
        if (config.FilterStatus != 0)
            query = query.Where(t => t.State != null && t.State.Oid == config.FilterStatus);
        if (config.FilterCategory != 0)
            query = query.Where(t => t.Category != null && t.Category.Oid == config.FilterCategory);
        if (config.FilterPriority != 0)
            query = query.Where(t => t.Preorety != null && t.Preorety.Oid == config.FilterPriority);
        if (config.FilterAuthor != 0)
            query = query.Where(t => t.Author != null && t.Author.Oid == config.FilterAuthor);
        if (config.FilterUser != 0)
            query = query.Where(t => t.User != null && t.User.Oid == config.FilterUser);
        if (config.FilterCompany != 0)
            query = query.Where(t => t.Company != null && t.Company.Oid == config.FilterCompany);

        return query;
    }

    private List<ReportRow> GroupBy(
        IQueryable<Tiket> query,
        ReportConfig config,
        Expression<Func<Tiket, string>> keySelector)
    {
        var rows = query
           .AsEnumerable()
        .GroupBy(keySelector.Compile())
        .Select(g => new ReportRow
        {
            Label = g.Key,
            Count = g.Count(),
            ClosedCount = g.Count(t => t.State != null && t.State.Oid == 2),
            AvgResolutionHours = g
                .Where(t => t.DueDate.HasValue)
                .Select(t => (t.DueDate!.Value - t.DataCreted).TotalHours)
                .DefaultIfEmpty(0)
                .Average(),
            AuthorOid = g.FirstOrDefault()?.Author?.Oid,
            AuthorName = g.FirstOrDefault()?.Author?.Name,
            UserOid = g.FirstOrDefault()?.User?.Oid,
            UserName = g.FirstOrDefault()?.User?.Name,
            State = g.FirstOrDefault()?.State?.Name,
            CompanyName = g.FirstOrDefault()?.Company?.Name,
            CompanyId = g.FirstOrDefault()?.Company?.Oid,
            OverdueCount = g.Count(t => IsOverdue(t)),
        })
        .ToDictionary(r => r.Label);

        // Получаем все возможные значения из БД
        var allLabels = GetAllLabels(config.GroupBy);

        // Мержим — если label есть в результате, берём его, иначе добавляем пустой
        var merged = allLabels.Select(label =>
            rows.TryGetValue(label, out var existing)
                ? existing
                : new ReportRow
                {
                    Label = label,
                    Count = 0,
                    ClosedCount = 0,
                    AvgResolutionHours = 0,
                    OverdueCount = 0,
                });

        return Sort(merged, config).ToList();

    }

    private List<ReportRow> GroupByMonth(IQueryable<Tiket> query, ReportConfig config)
    {
        var level = config.DateGrouping; // уровень группировки из конфига

    /*    // Фильтруем по сезону на уровне БД, если выбран конкретный сезон
        if (level == DateGroupingLevel.Spring || level == DateGroupingLevel.Summer ||
            level == DateGroupingLevel.Autumn || level == DateGroupingLevel.Winter)
        {
            string seasonName = level.ToString(); // "Spring", "Summer"...
                                                  // Преобразуем в русское название для проверки
            string seasonRu = level switch
            {
                DateGroupingLevel.Spring => "Весна",
                DateGroupingLevel.Summer => "Лето",
                DateGroupingLevel.Autumn => "Осень",
                DateGroupingLevel.Winter => "Зима",
                _ => ""
            };
            // Добавляем фильтр по месяцу, чтобы не тянуть все данные
            query = query.Where(t =>
                (seasonRu == "Весна" && t.DataCreted.Month >= 3 && t.DataCreted.Month <= 5) ||
                (seasonRu == "Лето" && t.DataCreted.Month >= 6 && t.DataCreted.Month <= 8) ||
                (seasonRu == "Осень" && t.DataCreted.Month >= 9 && t.DataCreted.Month <= 11) ||
                (seasonRu == "Зима" && (t.DataCreted.Month == 12 || t.DataCreted.Month <= 2))
            );
        }
    */
        var grouped = query
            .AsEnumerable() // материализуем отфильтрованные данные
            .GroupBy(t => GetDateGroupKey(t.DataCreted, level))
            .Select(g => new ReportRow
            {
                Label = g.Key,
                Count = g.Count(),
                ClosedCount = g.Count(t => t.State != null && t.State.Oid == 2),
                AvgResolutionHours = g.Where(t => t.DueDate.HasValue)
                                      .Select(t => (t.DueDate!.Value - t.DataCreted).TotalHours)
                                      .DefaultIfEmpty(0)
                                      .Average(),
                OverdueCount = g.Count(t => IsOverdue(t)),
                AuthorOid = g.FirstOrDefault()?.Author?.Oid,
                AuthorName = g.FirstOrDefault()?.Author?.Name,
                UserOid = g.FirstOrDefault()?.User?.Oid,
                UserName = g.FirstOrDefault()?.User?.Name,
                State = g.FirstOrDefault()?.State?.Name,
                CompanyName = g.FirstOrDefault()?.Company?.Name,
                CompanyId = g.FirstOrDefault()?.Company?.Oid,
                // Для сортировки используем минимальную дату в группе
                GroupKey = g.Min(t => t.DataCreted)
            });

        // Сортировка по хронологии (по GroupKey) или по количеству, если указано в SortBy
        var sorted = Sort(grouped, config);
        return sorted.ToList();
    }

    private List<ReportRow> GetFlatList(IEnumerable<Tiket> query)
    {
        return query
            .AsEnumerable()
            .Select(t => new ReportRow
            {
                Label = t.Title ?? "(без названия)",
                Count = 1,
                ClosedCount = t.State?.Oid == 2 ? 1 : 0,
                AvgResolutionHours = t.DueDate.HasValue
                    ? (t.DueDate.Value - t.DataCreted).TotalHours
                    : 0,
                GroupKey = t.Oid,
                AuthorOid = t.Author?.Oid,
                AuthorName = t.Author?.Name,
                UserOid = t.User?.Oid,
                CompanyId = t.Company?.Oid,
                CompanyName = t.Company?.Name,
                UserName = t.User?.Name,
                OverdueCount = IsOverdue(t) ? 1 : 0,
            })
            .ToList();
    }

    private static IEnumerable<ReportRow> Sort(IEnumerable<ReportRow> rows, ReportConfig config)
    {
        if (config.GroupBy == ReportGroupBy.Date)
        {
            // Если явно указана сортировка по Count
            if (config.SortBy == ReportSortBy.Count)
            {
                return config.SortDescending
                    ? rows.OrderByDescending(r => r.Count)
                    : rows.OrderBy(r => r.Count);
            }
            // Иначе сортируем хронологически
            return config.SortDescending
                ? rows.OrderByDescending(r => r.GroupKey)
                : rows.OrderBy(r => r.GroupKey);
        }

        return (config.SortBy, config.SortDescending) switch
        {
            (ReportSortBy.Count, true) => rows.OrderByDescending(r => r.Count),
            (ReportSortBy.Count, false) => rows.OrderBy(r => r.Count),
            (ReportSortBy.Name, true) => rows.OrderByDescending(r => r.Label),
            (ReportSortBy.Name, false) => rows.OrderBy(r => r.Label),
            _ => rows
        };
    }

    private List<string> GetAllLabels(ReportGroupBy groupBy)
    {
        return groupBy switch
        {
            ReportGroupBy.State => _uow.Query<State>()
            .Where(s => s.Active == true)
            .Select(s => s.Name)
            .ToList(),

            ReportGroupBy.Category => _uow.Query<Category>()
                .Where(c => c.Active == true)
                .Select(c => c.Name)
                .ToList(),

            ReportGroupBy.Priority => _uow.Query<Preority>()
                .Where(p => p.Active == true)
                .Select(p => p.Name)
                .ToList(),

            ReportGroupBy.Author => _uow.Query<Author>()
                .Where(a => a.Active == true)
                .Select(a => a.Name)
                .ToList(),
            ReportGroupBy.User => _uow.Query<User>()
           .Where(a => a.IsActive == true)
           .Select(a => a.Name)
           .ToList(),

            ReportGroupBy.Company => _uow.Query<Company>()
           .Where(a => a.Active == true)
           .Select(a => a.Name)
           .ToList(),

            _ => new List<string>()
        };
    }
    private static bool IsOverdue(Tiket t)
    {

        return t.DueDate.HasValue && t.DueDate.Value < DateTime.Now;
    }

    // даты для сортировки
    private string GetDateGroupKey(DateTime date, DateGroupingLevel level)
    {
        return level switch
        {
            DateGroupingLevel.Day => date.ToString("yyyy-MM-dd"),
            DateGroupingLevel.Week => GetWeekKey(date),
            DateGroupingLevel.Month => date.ToString("yyyy-MM"),
            DateGroupingLevel.Year => date.ToString("yyyy"),
            DateGroupingLevel.Spring => GetSeasonKey(date, "Весна"),
            DateGroupingLevel.Summer => GetSeasonKey(date, "Лето"),
            DateGroupingLevel.Autumn => GetSeasonKey(date, "Осень"),
            DateGroupingLevel.Winter => GetSeasonKey(date, "Зима"),
            _ => date.ToString("yyyy-MM")
        };
    }

    // Формат ключа для недели: "2026-W15"
    private string GetWeekKey(DateTime date)
    {
        var culture = System.Globalization.CultureInfo.CurrentCulture;
        var week = culture.Calendar.GetWeekOfYear(
            date,
            System.Globalization.CalendarWeekRule.FirstFourDayWeek,
            DayOfWeek.Monday);
        return $"{date.Year}-W{week:D2}";
    }

    // Для сезонов возвращаем год и название сезона, чтобы различать весну 2025 и весну 2026
    private string GetSeasonKey(DateTime date, string seasonName)
    {
        // Определяем год, к которому относится сезон.
        // Например, зима 2025/2026 относится к 2025 году.
        int year = date.Year;
        // Для зимы декабрь, январь, февраль: если месяц январь-февраль, считаем предыдущий год
        if (seasonName == "Зима" && date.Month <= 2)
            year = date.Year - 1;
        // Для весны март-май, лета июнь-август, осени сентябрь-ноябрь — год совпадает

        return $"{year} {seasonName}";
    }

    private bool IsDateInSeason(DateTime date, string season)
    {
        return season switch
        {
            "Весна" => date.Month >= 3 && date.Month <= 5,
            "Лето" => date.Month >= 6 && date.Month <= 8,
            "Осень" => date.Month >= 9 && date.Month <= 11,
            "Зима" => date.Month == 12 || date.Month <= 2,
            _ => false
        };
    }

}