using DevExpress.Xpo;

[Persistent("Category")]
public class Category: BaseEntity
{
    public Category(Session session) : base(session) { }

    [Association("Category-Tikets")]
    public XPCollection<Tiket> Tikets => GetCollection<Tiket>(nameof(Tikets));

    [Association("SubCategory-Categorys")]
    public XPCollection<SubCategory> SubCategorys => GetCollection<SubCategory>(nameof(SubCategorys));
}
