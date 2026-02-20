using DevExpress.Xpo;

[Persistent("Category")]
public class Category: BaseEntity
{
    private SubCategory _subCategory;
    public Category(Session session) : base(session) { }

    [Association("Category-Tikets")]
    public XPCollection<Tiket> Tikets => GetCollection<Tiket>(nameof(Tikets));

    [Association("SubCategory-Categorys")]
    public SubCategory SubCategory
    {
        get => _subCategory;
        set => SetPropertyValue(nameof(SubCategory), ref _subCategory, value);
    }


}
