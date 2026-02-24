using DevExpress.Xpo;

[Persistent("Platform")]
public class Platform: BaseEntity
{
    public Platform(Session session): base(session) { }

    private Company _company;

    [Association("Platform-Authors")]
    public XPCollection<Author> Authors => GetCollection<Author>(nameof(Authors));

    [Association("Platforms-Companys")]
   public Company Company
    {
        get => _company;
        set => SetPropertyValue(nameof(Company), ref _company, value);
    }

    [Association("Platform-Tikets")]
    public XPCollection<Tiket> Tikets => GetCollection<Tiket>(nameof(Tikets));



}
