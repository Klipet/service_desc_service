using DevExpress.Xpo;

[Persistent("Preority")]
public class Preority : BaseEntity
{
    private int _deadlineHours;
    public Preority(Session session) : base(session) { }

    [Association("Preority-Tikets")]
    public XPCollection<Tiket> Tikets => GetCollection<Tiket>(nameof(Tikets));

    public int DeadlineHours
    {
        get => _deadlineHours;
        set => SetPropertyValue(nameof(DeadlineHours), ref _deadlineHours, value);
    }

    [NonPersistent]
    public string DeadlineLabel => DeadlineHours <= 24
        ? $"{DeadlineHours} ч."
        : $"{DeadlineHours / 24} дн.";

}

