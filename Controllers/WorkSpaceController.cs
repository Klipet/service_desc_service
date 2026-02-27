
using DevExpress.Xpo;
using Microsoft.AspNetCore.Mvc;

public class WorkSpaceController : BaseController<WorkSpace, WorkSpaceDto>
{
    public WorkSpaceController(UnitOfWork uow): base(uow) { }
    protected override WorkSpace CreateModel(WorkSpaceDto dto) => new WorkSpace(_uow)
    {
        Name = dto.Name,
        Active = dto.Active,
        DateCreated = DateTime.UtcNow,
        DateModified = DateTime.UtcNow,
    };
}

