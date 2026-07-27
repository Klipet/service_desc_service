using DevExpress.Xpo;

[Persistent("TicketPhone")]
public class TicketPhone : XPObject
{
    public TicketPhone(Session session) : base(session) { }

    private Tiket _ticket;
    private User _user;
    private string _phone;
    private PhoneResult _resultPhone;
    private string _comment;
    private DateTime _dataCreated;
    private DateTime _dataModified;
    private bool _isActive = true;

    [Association("Ticket-Phones")]
    public Tiket Ticket
    {
        get => _ticket;
        set => SetPropertyValue(nameof(Ticket), ref _ticket, value);
    }

    [Association("PhoneResults-TicketPhone")]
    public PhoneResult PhoneResults
    {
        get => _resultPhone;
        set => SetPropertyValue(nameof(PhoneResults), ref _resultPhone, value);
    }

    public User User
    {
        get => _user;
        set => SetPropertyValue(nameof(User), ref _user, value);
    }

    [Size(50)]
    public string Phone
    {
        get => _phone;
        set => SetPropertyValue(nameof(Phone), ref _phone, value);
    }

    [Size(SizeAttribute.Unlimited)]
    public string Comment
    {
        get => _comment;
        set => SetPropertyValue(nameof(Comment), ref _comment, value);
    }

    public DateTime DataCreated
    {
        get => _dataCreated;
        set => SetPropertyValue(nameof(DataCreated), ref _dataCreated, value);
    }

    public DateTime DataModified
    {
        get => _dataModified;
        set => SetPropertyValue(nameof(DataModified), ref _dataModified, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetPropertyValue(nameof(IsActive), ref _isActive, value);
    }

    protected override void OnSaving()
    {
        base.OnSaving();
        DataModified = DateTime.Now;
    }
}
