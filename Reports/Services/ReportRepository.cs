using DevExpress.Xpo;

public class ReportRepository : IReportRepository
{
    private readonly UnitOfWork _uow;

    public ReportRepository(UnitOfWork uow) => _uow = uow;

    public Task<int> SaveConfigAsync(ReportRequestDto request)
    {
        var state = _uow.Query<Tiket>().FirstOrDefault(t => t.State.Oid == request.FilterStatus);
        var category = _uow.Query<Tiket>().FirstOrDefault(t => t.Category.Oid == request.FilterCategory);
        var preority = _uow.Query<Tiket>().FirstOrDefault(t => t.Preorety.Oid == request.FilterPriority);
        var author = _uow.Query<Tiket>().FirstOrDefault(t => t.Author.Oid == request.FilterAuthorId);

        var config = new ReportConfig(_uow)
        {
            Name = request.Name,
            DateFrom = request.DateFrom,
            DateTo = request.DateTo,
            ShowTotalCount = request.ShowTotalCount,
            FilterStatus = state.Oid,
            FilterCategory = category.Oid,
            FilterPriority = preority.Oid,
            SortDescending = request.SortDescending,
            CreatedAt = DateTime.Now,
            GroupBy = Enum.TryParse<ReportGroupBy>(request.GroupBy, out var g) ? g : ReportGroupBy.None,
            SortBy = Enum.TryParse<ReportSortBy>(request.SortBy, out var s) ? s : ReportSortBy.Count,
        };

    //    if (request.FilterAuthorId.HasValue)
     //       config.FilterAuthor = _uow.GetObjectByKey<User>(request.FilterAuthorId.Value);

        _uow.Save(config);
        _uow.CommitChanges();

        return Task.FromResult(config.Oid);
    }

    public Task<List<ReportConfigDto>> GetAllAsync()
    {
        var list = new XPCollection<ReportConfig>(_uow)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ReportConfigDto
            {
                Id = c.Oid,
                Name = c.Name,
                CreatedAt = c.CreatedAt,
                GroupBy = c.GroupBy.ToString(),
                DateFrom = c.DateFrom,
                DateTo = c.DateTo,
            })
            .ToList();

        return Task.FromResult(list);
    }

    public Task<ReportConfigDto?> GetByIdAsync(int id)
    {
        var c = _uow.GetObjectByKey<ReportConfig>(id);
        if (c == null) return Task.FromResult<ReportConfigDto?>(null);

        return Task.FromResult<ReportConfigDto?>(new ReportConfigDto
        {
            Id = c.Oid,
            Name = c.Name,
            CreatedAt = c.CreatedAt,
            GroupBy = c.GroupBy.ToString(),
            DateFrom = c.DateFrom,
            DateTo = c.DateTo,
        });
    }

    public Task<bool> DeleteAsync(int id)
    {
        var config = _uow.GetObjectByKey<ReportConfig>(id);
        if (config == null) return Task.FromResult(false);

        _uow.Delete(config);
        _uow.CommitChanges();
        return Task.FromResult(true);
    }
}
