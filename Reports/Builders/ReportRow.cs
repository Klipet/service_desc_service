public class ReportRow
{
    public string Label { get; set; }
    public int Count { get; set; }
    public int? ClosedCount { get; set; }
    public double? AvgResolutionHours { get; set; }
    public int? AuthorOid { get; set; }
    public string AuthorName { get; set; }
    public int? UserOid { get; set; }
    public int OverdueCount { get; set; }
    public string CompanyName { get; set; }
    public int? CompanyId { get; set; }
    public string UserName { get; set; }
    public object GroupKey { get; set; }
    public string State {  get; set; }
}

public class ReportResult
{
    public int TotalCount { get; set; }
    public List<ReportRow> Rows { get; set; } = new();
}
