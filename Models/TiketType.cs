using DevExpress.Xpo;

[Persistent("TypeTiket")]
public class TiketType: BaseEntity
{
    public TiketType(Session session) : base(session) {}
    [Association("TiketType-Tikets")]
    public XPCollection<Tiket> Tikets => GetCollection<Tiket>(nameof(Tikets));
}
