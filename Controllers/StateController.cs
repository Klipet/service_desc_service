using DevExpress.Xpo;
using System;
using System.Xml.Linq;

public class StateController : BaseController<State, StateDto>
{
    protected override string PermissionRead => PermisionConstant.StateRead;
    protected override string PermissionCreate => PermisionConstant.StateCreate;
    protected override string PermissionUpdate => PermisionConstant.StateUpdate;
    protected override string PermissionDelete => PermisionConstant.StateDelete;
    public StateController(UnitOfWork uow) : base(uow) { }
    protected override State CreateModel(StateDto dto) => new State(_uow)
    {
        Name = dto.Name,
        Active = dto.Active,
        DateCreated = DateTime.UtcNow,
        DateModified = DateTime.UtcNow,
    };
}
