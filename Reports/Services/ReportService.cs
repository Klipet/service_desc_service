using ClosedXML.Excel;
using DevExpress.Xpo;

public class ReportService : IReportService
{
    private readonly UnitOfWork _uow;

    public ReportService(UnitOfWork uow) => _uow = uow;

    public Task<ReportResultDto> BuildAsync(ReportRequestDto request)
    {
        var config = new ReportConfig(_uow)
        {
            Name = request.Name,
            DateFrom = request.DateFrom,
            DateTo = request.DateTo,
            ShowTotalCount = request.ShowTotalCount,
            SortDescending = request.SortDescending,
            GroupBy = Enum.TryParse<ReportGroupBy>(request.GroupBy, out var g) ? g : ReportGroupBy.None,
            SortBy = Enum.TryParse<ReportSortBy>(request.SortBy, out var s) ? s : ReportSortBy.Count,
            FilterStatus = request.FilterStatus ?? 0,
            FilterCategory = request.FilterCategory ?? 0,
            FilterPriority = request.FilterPriority ?? 0,
            FilterAuthor = request.FilterAuthorId ?? 0,
            FilterUser = request.FilterUserId ?? 0,
        };

        




        var internal_ = new ReportQueryBuilder(_uow).Build(config);

        var dto = new ReportResultDto
        {
            ReportName = config.Name ?? "Отчёт",
            GeneratedAt = DateTime.Now,
            TotalCount = internal_.TotalCount,
            GroupBy = request.GroupBy,
            Rows = internal_.Rows.Select(r => new ReportRowDto
            {
                Label = r.Label,
                Count = r.Count,
                AuthorOid = r.AuthorOid,
                AuthorName = r.AuthorName,
                UserOid = r.UserOid,
                UserName = r.UserName,
                ClosedCount = r.ClosedCount,
                AvgResolutionHours = r.AvgResolutionHours
            }).ToList()
        };

        return Task.FromResult(dto);
    }

    public Task<byte[]> ExportToExcelAsync(ReportResultDto result)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add(result.ReportName ?? "Отчёт");

        ws.Cell(1, 1).Value = "Группа";
        ws.Cell(1, 2).Value = "Количество";
        ws.Row(1).Style.Font.Bold = true;

        for (int i = 0; i < result.Rows.Count; i++)
        {
            ws.Cell(i + 2, 1).Value = result.Rows[i].Label;
            ws.Cell(i + 2, 2).Value = result.Rows[i].Count;
        }

        int last = result.Rows.Count + 2;
        ws.Cell(last, 1).Value = "Итого";
        ws.Cell(last, 2).Value = result.TotalCount;
        ws.Row(last).Style.Font.Bold = true;
        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return Task.FromResult(ms.ToArray());
    }
}