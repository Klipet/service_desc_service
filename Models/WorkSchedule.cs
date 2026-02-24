using DevExpress.Xpo;

public class WorkSchedule: XPObject
{
    public WorkSchedule(Session session) : base(session) { }

    private DayOfWeek _dayOfWeek;
    private TimeSpan _endTime;
    private TimeSpan _startTime;
    private bool _isWorkingDay;


    public DayOfWeek DayOfWeek
    {
        get => _dayOfWeek;
        set => SetPropertyValue(nameof(DayOfWeek), ref _dayOfWeek, value);
    }
    public TimeSpan StartTime
    {
        get => _startTime;
        set => SetPropertyValue(nameof(StartTime), ref _startTime, value);
    }
    public TimeSpan EndTime
    {
        get => _endTime;
        set => SetPropertyValue(nameof(EndTime), ref _endTime, value);
    }
    public bool IsWorkingDay
    {
        get => _isWorkingDay;
        set => SetPropertyValue(nameof(IsWorkingDay), ref _isWorkingDay, value);
    }

    [NonPersistent]
    public string DayName => DayOfWeek switch
    {
        DayOfWeek.Monday => "Понедельник",
        DayOfWeek.Tuesday => "Вторник",
        DayOfWeek.Wednesday => "Среда",
        DayOfWeek.Thursday => "Четверг",
        DayOfWeek.Friday => "Пятница",
        DayOfWeek.Saturday => "Суббота",
        DayOfWeek.Sunday => "Воскресенье",
        _ => ""
    };
}
