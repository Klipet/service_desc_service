using DevExpress.Xpo;

[Persistent("TypeTiket")]
public class TiketType: BaseEntity
{
    public TiketType(Session session) : base(session) {}
    [Association("TiketType-Tikets")]
    public XPCollection<NewTiket> Tikets => GetCollection<NewTiket>(nameof(Tikets));
}
