using DevExpress.Xpo;
using DocumentFormat.OpenXml.ExtendedProperties;
using System.Runtime.InteropServices;

[Persistent("Company")]
public class Company: BaseEntity
{
    public Company(Session session) : base(session) { }

    public Platform _platform;
    public CompanyState _companyState;
    public string _idnp;

    [Association("Company-Tikets")]
    public XPCollection<Tiket> Tikets => GetCollection<Tiket>(nameof(Tikets));

    [Association("Platforms-Companys")]
    public XPCollection<Platform> Platforms => GetCollection<Platform>(nameof(Platforms));

    [Association("CompanysState-Companys")]
    public CompanyState CompanyState
    {
        get => _companyState;
        set => SetPropertyValue(nameof(CompanyState), ref _companyState, value);
    }

    public string Idnp
    {
        get => _idnp;
        set => SetPropertyValue(nameof(Idnp), ref _idnp, value);
    }

}