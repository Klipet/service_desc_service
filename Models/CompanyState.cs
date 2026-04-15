
using DevExpress.Xpo;

[Persistent("CompanyState")]
public class CompanyState: BaseEntity
{
    public CompanyState(Session session) : base(session){}


    [Association("CompanysState-Companys")]
    public XPCollection<Company> Companys => GetCollection<Company>(nameof(Companys));

}
