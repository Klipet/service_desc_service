public class ReportRequestDto
{
    public string Name { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public bool ShowTotalCount { get; set; }
    public int? FilterAuthorId { get; set; } 
    public int? FilterStatus { get; set; }
    public int? FilterCategory { get; set; } 
    public int? FilterPriority { get; set; } 
    public int? FilterUserId { get; set; }  
    public string GroupBy { get; set; }
    public string SortBy { get; set; }
    public bool SortDescending { get; set; }
}

public class ReportRowDto
{
    public string Label { get; set; }
    public int Count { get; set; }
    public int? ClosedCount { get; set; }
    public string AuthorName { get; set; }
    public int? AuthorOid { get; set; }
    public int? UserOid { get; set; }
    public string UserName { get; set; } 
    public double? AvgResolutionHours { get; set; }
}

public class ReportResultDto
{
    public string ReportName { get; set; }
    public DateTime GeneratedAt { get; set; }
    public int TotalCount { get; set; }
    public string GroupBy { get; set; }
    public List<ReportRowDto> Rows { get; set; } = new();
}

public class ReportConfigDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public string GroupBy { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}