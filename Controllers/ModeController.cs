using DevExpress.Xpo;

public class ModeController: BaseController<Mode, ModeDto>
{
    public ModeController(UnitOfWork uow): base(uow) { }

    protected override Mode CreateModel(ModeDto dto) => new Mode(_uow)
    {
        Name = dto.Name,
        Active = dto.Active,
        DateCreated = DateTime.UtcNow,
        DateModified = DateTime.UtcNow,

    };

}

