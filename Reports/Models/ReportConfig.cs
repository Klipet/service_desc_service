using DevExpress.Xpo;

[Persistent("ReportConfigs")]
public class ReportConfig : XPObject
{
    public ReportConfig(Session session) : base(session) { }
    public ReportConfig() : base() { }

    private string _name;
    private DateTime? _dateFrom;
    private DateTime? _dateTo;
    private DateTime _createdAt;
    private bool _showTotalCount;
    private int? _filterStatus;
    private int? _filterCategory;
    private int? _filterPriority;
    private ReportGroupBy _groupBy;
    private ReportSortBy _sortBy;
    private bool _sortDescending;
    private int? _filterAuthor;
    public int? _filterUser;
    public int? _filterCompany;
    private DateGroupingLevel _dateGrouping;

    [Persistent]
    public DateGroupingLevel DateGrouping
    { get => _dateGrouping; set => SetPropertyValue(nameof(DateGrouping), ref _dateGrouping, value); }

    [Persistent]
    public int? FilterCompany
    { get => _filterCompany; set => SetPropertyValue(nameof(FilterCompany), ref _filterCompany, value); }

    [Persistent]
    public int? FilterUser
    { get => _filterUser; set => SetPropertyValue(nameof(FilterUser), ref _filterUser, value); }

    [Persistent]
    public string Name
    { get => _name; set => SetPropertyValue(nameof(Name), ref _name, value); }

    [Persistent]
    public DateTime? DateFrom
    { get => _dateFrom; set => SetPropertyValue(nameof(DateFrom), ref _dateFrom, value); }

   
    [Persistent]
    public DateTime? DateTo
    { get => _dateTo; set => SetPropertyValue(nameof(DateTo), ref _dateTo, value); }

    [Persistent]
    public DateTime CreatedAt
    { get => _createdAt; set => SetPropertyValue(nameof(CreatedAt), ref _createdAt, value); }

    [Persistent]
    public bool ShowTotalCount
    { get => _showTotalCount; set => SetPropertyValue(nameof(ShowTotalCount), ref _showTotalCount, value); }

    [Persistent]
    public int? FilterStatus
    { get => _filterStatus; set => SetPropertyValue(nameof(FilterStatus), ref _filterStatus, value); }


    [Persistent]
    public int? FilterCategory
    { get => _filterCategory; set => SetPropertyValue(nameof(FilterCategory), ref _filterCategory, value); }


    [Persistent]
    public int? FilterPriority
    { get => _filterPriority; set => SetPropertyValue(nameof(FilterPriority), ref _filterPriority, value); }


    [Persistent]
    public ReportGroupBy GroupBy
    { get => _groupBy; set => SetPropertyValue(nameof(GroupBy), ref _groupBy, value); }


    [Persistent]
    public ReportSortBy SortBy
    { get => _sortBy; set => SetPropertyValue(nameof(SortBy), ref _sortBy, value); }


    [Persistent]
    public bool SortDescending
    { get => _sortDescending; set => SetPropertyValue(nameof(SortDescending), ref _sortDescending, value); }


    [Persistent]
    public int? FilterAuthor
    { get => _filterAuthor; set => SetPropertyValue(nameof(FilterAuthor), ref _filterAuthor, value); }
}