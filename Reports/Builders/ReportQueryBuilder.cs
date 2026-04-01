using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using Org.BouncyCastle.Asn1.Ocsp;

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
            ReportGroupBy.Date => GroupByMonth(query, config),
            _ => GetFlatList(query)
        };

        return result;
    }

    private IEnumerable<Tiket> BuildQuery(ReportConfig config)
    {
        var query = _uow.Query<Tiket>().AsEnumerable();

        if (config.DateFrom.HasValue)
            query = query.Where(t => t.DataCreted >= config.DateFrom.Value);

        if (config.DateTo.HasValue)
            query = query.Where(t => t.DataCreted < config.DateTo.Value.Date.AddDays(1));

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

        return query;
    }

    private List<ReportRow> GroupBy(
        IEnumerable<Tiket> query,
        ReportConfig config,
        Func<Tiket, string> keySelector)
    {
        var rows = query
            .GroupBy(keySelector)
            .Select(g => new ReportRow
            {
                Label = g.Key,
                Count = g.Count(),
                ClosedCount = g.Count(t => t.State != null && t.State.Oid == 2),
                AvgResolutionHours = g
                    .Where(t => t.DueDate.HasValue)
                    .Select(t => (t.DueDate!.Value - t.DataCreted).TotalHours)
                    .DefaultIfEmpty(0)
                    .Average()
            });

        return Sort(rows, config).ToList();
    }

    private List<ReportRow> GroupByMonth(IEnumerable<Tiket> query, ReportConfig config)
    {
        return query
            .GroupBy(t => new { t.DataCreted.Year, t.DataCreted.Month })
            .Select(g => new ReportRow
            {
                Label = $"{g.Key.Year}-{g.Key.Month:D2}",
                Count = g.Count(),
                ClosedCount = g.Count(t => t.State != null && t.State.Name == "Закрыт"),
                AvgResolutionHours = g
                    .Where(t => t.DueDate.HasValue)
                    .Select(t => (t.DueDate!.Value - t.DataCreted).TotalHours)
                    .DefaultIfEmpty(0)
                    .Average(),
                AuthorOid = g.FirstOrDefault()?.Author?.Oid,
                AuthorName = g.FirstOrDefault()?.Author?.Name,
                UserOid = g.FirstOrDefault()?.User?.Oid,
                UserName = g.FirstOrDefault()?.User?.Name,
                GroupKey = new DateTime(g.Key.Year, g.Key.Month, 1)
            })
            .OrderBy(r => r.GroupKey)
            .ToList();
    }

    private List<ReportRow> GetFlatList(IEnumerable<Tiket> query)
    {
        return query
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
                UserName = t.User?.Name,
            })
            .ToList();
    }

    private static IEnumerable<ReportRow> Sort(IEnumerable<ReportRow> rows, ReportConfig config)
    {
        return (config.SortBy, config.SortDescending) switch
        {
            (ReportSortBy.Count, true) => rows.OrderByDescending(r => r.Count),
            (ReportSortBy.Count, false) => rows.OrderBy(r => r.Count),
            (ReportSortBy.Name, true) => rows.OrderByDescending(r => r.Label),
            (ReportSortBy.Name, false) => rows.OrderBy(r => r.Label),
            _ => rows
        };
    }
}