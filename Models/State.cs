using DevExpress.Xpo;

[Persistent("State")]
public class State: BaseEntity
{
    public State(Session session) : base(session){}

    [Association("State-Tikets")]
    public XPCollection<Tiket> Tikets => GetCollection<Tiket>(nameof(Tikets));

}

