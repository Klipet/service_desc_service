using DevExpress.Xpo;

public class CompanyStateController: BaseController<CompanyState, CompanyStateDto>
{
    protected override string PermissionRead => PermisionConstant.CompanyRead;
    protected override string PermissionCreate => PermisionConstant.CompanyCreate;
    protected override string PermissionUpdate => PermisionConstant.CompanyUpdate;
    protected override string PermissionDelete => PermisionConstant.CompanyDelete;

    public CompanyStateController(UnitOfWork uow): base(uow) { }
    protected override CompanyState CreateModel(CompanyStateDto dto) => new CompanyState(_uow)
    {
        Name = dto.Name,
        Active = dto.Active,
        DateCreated = DateTime.UtcNow,
        DateModified = DateTime.UtcNow,
    };
}