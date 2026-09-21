using DevExpress.Xpo;

[Persistent("SubCategory")]
public class SubCategory : BaseEntity
{
    private Category _category;
    public SubCategory(Session session) : base(session) { }

    [Association("SubCategory-Tikets")]
    public XPCollection<Tiket> Tikets => GetCollection<Tiket>(nameof(Tikets));

    [Association("SubCategory-Categorys")]
    public Category Category
    {
        get => _category;
        set => SetPropertyValue(nameof(Category), ref _category, value);
    }
}

