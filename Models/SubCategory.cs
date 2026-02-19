using DevExpress.Xpo;

[Persistent("SubCategory")]
public class SubCategory: BaseEntity
{
    public SubCategory(Session session) : base(session) { }

    [Association("SubCategory-Tikets")]
    public XPCollection<NewTiket> Tikets => GetCollection<NewTiket>(nameof(Tikets));

    [Association("SubCategory-Categorys")]
    public XPCollection<Category> Categorys => GetCollection<Category>(nameof(Categorys));
}

