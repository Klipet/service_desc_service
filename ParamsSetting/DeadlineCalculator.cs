using DevExpress.Xpo;

public class DeadlineCalculator
{
    private readonly Session _session;

    public DeadlineCalculator(Session session)
    {
        _session = session;
    }

    public DateTime Calculate( int deadlineHours)
    {
        var holidays = GetHolidays();
        var schedule = GetSchedule();

        var current = DateTime.UtcNow;
        var hoursLeft = deadlineHours;

        int safetyCounter = 0;

        while (hoursLeft > 0)
        {
            current = current.AddHours(1);

            if (IsWorkingHour(current, holidays, schedule))
                Console.WriteLine($"Working hour: {current}");
                hoursLeft--;

            safetyCounter++;

            if (safetyCounter > 10000)
                throw new Exception("Infinite loop detected in deadline calculation");
        }

        return current;
    }

    private bool IsWorkingHour(DateTime dt, HashSet<string> holidays, Dictionary<DayOfWeek, WorkSchedule> schedule)
    {
        // Нет расписания для этого дня
        if (!schedule.TryGetValue(dt.DayOfWeek, out var day))
            return false;

        // Не рабочий день
        if (!day.IsWorkingDay)
            return false;

        // Праздник (ежегодный)
        if (holidays.Contains(dt.ToString("dd.MM")))
            return false;

        // Праздник (конкретная дата)
        if (holidays.Contains(dt.ToString("dd.MM.yyyy")))
            return false;

        // Не рабочее время
        var time = dt.TimeOfDay;
        if (time < day.StartTime || time >= day.EndTime)
            return false;

        return true;
    }

    private HashSet<string> GetHolidays()
    {
        using var uow = MyXPO.GetNewUnitOfWork();

        var set = new HashSet<string>();

        var holidays = uow.Query<HolidayDay>().ToList();

        foreach (var h in holidays)
        {
            if (h.IsRecurringYearly)
                set.Add(h.Date.ToString("dd.MM"));
            else
                set.Add(h.Date.ToString("dd.MM.yyyy"));
        }

        return set;
    }

    private Dictionary<DayOfWeek, WorkSchedule> GetSchedule()
    {
        using var uow = MyXPO.GetNewUnitOfWork();
        return new XPCollection<WorkSchedule>(uow)
            .ToDictionary(s => s.DayOfWeek);
    }
}