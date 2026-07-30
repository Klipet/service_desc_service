using DevExpress.Xpo;

public class PhoneResaultController: BaseController<PhoneResult, PhoneResaultDto>
{
    protected override string PermissionRead => PermisionConstant.StateRead;
    protected override string PermissionCreate => PermisionConstant.StateCreate;
    protected override string PermissionUpdate => PermisionConstant.StateUpdate;
    protected override string PermissionDelete => PermisionConstant.StateDelete;
    public PhoneResaultController(UnitOfWork uow) : base(uow) { }
    protected override PhoneResult CreateModel(PhoneResaultDto dto) => new PhoneResult(_uow)
    {
        Name = dto.Name,
        Active = dto.Active,
        DateCreated = DateTime.UtcNow,
        DateModified = DateTime.UtcNow,
    };
}

