using DevExpress.Xpo;

public class TiketTypeController: BaseController<TiketType, TiketTypeDto>
{
    public TiketTypeController(UnitOfWork uow) : base(uow) { }
    protected override string PermissionRead => PermisionConstant.TiketTypeRead;
    protected override string PermissionCreate => PermisionConstant.TiketTypeCreate;
    protected override string PermissionUpdate => PermisionConstant.TiketTypeUpdate;
    protected override string PermissionDelete => PermisionConstant.TiketTypeDelete;
    protected override TiketType CreateModel(TiketTypeDto dto) => new TiketType(_uow)
    {
        Name = dto.Name,
        Active = dto.Active,
        DateCreated = DateTime.UtcNow,
        DateModified = DateTime.UtcNow,
    };
}
