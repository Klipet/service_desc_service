using DevExpress.Xpo;
using System.Runtime.InteropServices;

[Persistent("Company")]
public class Company: BaseEntity
{
    public Company(Session session) : base(session) { }

    public Platform _platform;
    public bool _vipState;
    public string _idnp;

    [Association("Company-Tikets")]
    public XPCollection<Tiket> Tikets => GetCollection<Tiket>(nameof(Tikets));

    [Association("Platform-Companys")]
    public Platform Platform
    {
        get => _platform;
        set => SetPropertyValue(nameof(Platform), ref _platform, value);
    }
    public bool VipState
    {
        get => _vipState;
        set => SetPropertyValue(nameof(VipState), ref _vipState, value);
    }
    public string Idnp
    {
        get => _idnp;
        set => SetPropertyValue(nameof(Idnp), ref _idnp, value);
    }

}