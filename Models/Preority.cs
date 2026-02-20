using DevExpress.Xpo;

[Persistent("Preority")]
public class Preority : BaseEntity
{
    public Preority(Session session) : base(session) { }

    [Association("Preority-Tikets")]
    public XPCollection<Tiket> Tikets => GetCollection<Tiket>(nameof(Tikets));

}

