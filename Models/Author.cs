using DevExpress.Xpo;
using System.Runtime.InteropServices;

[Persistent("Autor")]

public class Author : BaseEntity
{
    private Platform _platform;
    private string _email;


    public Author(Session session) : base(session) { }

    [Association("Author-Tikets")]
    public XPCollection<Tiket> Tikets => GetCollection<Tiket>(nameof(Tikets));

    [Association("Platform-Authors")]
    public Platform Platform
    {
        get => _platform;
        set => SetPropertyValue(nameof(Platform), ref _platform, value);
    }
    public string Email
    {
        get => _email;
        set => SetPropertyValue(nameof(Email), ref _email, value);
    }

    [Association("Author-Phones")]
    [Aggregated]
    public XPCollection<Phone> Phones
    {
        get { return GetCollection<Phone>(nameof(Phones)); }
    }
}
