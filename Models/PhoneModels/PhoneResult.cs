using DevExpress.Xpo;

public class PhoneResult : BaseEntity
{
    public PhoneResult(Session session) : base(session) { }

    [Association("PhoneResults-TicketPhone")]
    public XPCollection<TicketPhone> PhoneResults => GetCollection<TicketPhone>(nameof(TicketPhone.PhoneResults));
}
