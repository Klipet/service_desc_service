using DevExpress.Xpo;
using Microsoft.AspNetCore.Mvc;


public class ModeController: BaseController<Mode, ModeDto>
{
    public ModeController(UnitOfWork uow): base(uow) { }
    protected override string PermissionRead => PermisionConstant.ModeRead;
    protected override string PermissionCreate => PermisionConstant.ModeCreate;
    protected override string PermissionUpdate => PermisionConstant.ModeUpdate;
    protected override string PermissionDelete => PermisionConstant.ModeDelete;

    protected override Mode CreateModel(ModeDto dto) => new Mode(_uow)
    {
        Name = dto.Name,
        Active = dto.Active,
        DateCreated = DateTime.UtcNow,
        DateModified = DateTime.UtcNow,
    };

}

