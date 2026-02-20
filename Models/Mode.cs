using DevExpress.Xpo;
[Persistent("Mode")]
public class Mode: BaseEntity
{
    public Mode(Session session) : base(session) { }
    [Association("Mode-Tikets")]
    public XPCollection<Tiket> Tikets => GetCollection<Tiket>(nameof(Tikets));
}
