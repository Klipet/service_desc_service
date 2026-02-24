using DevExpress.Xpo;

public class HolidayDay: XPObject
{
    public HolidayDay(Session session) : base(session) { }

    private string _name;
    private DateTime _date;
    private bool _isRecurringYearly;


    public string Name
    {
        get => _name;
        set => SetPropertyValue(nameof(Name), ref _name, value);
    }
    public DateTime Date
    {
        get => _date;
        set => SetPropertyValue(nameof(Date), ref _date, value);
    }
    public bool IsRecurringYearly
    {
        get => _isRecurringYearly;
        set => SetPropertyValue(nameof(IsRecurringYearly), ref _isRecurringYearly, value);
    }
}

