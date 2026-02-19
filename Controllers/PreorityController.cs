using DevExpress.Xpo;

public class PreorityController : BaseController<Preority, PreorityDto>
{
    public PreorityController(UnitOfWork uow) : base(uow) { }
    protected override Preority CreateModel(PreorityDto dto) => new Preority(_uow)
    {
        Name = dto.Name,
        Active = dto.Active,
        DateCreated = dto.DateCreated,
        DateModified = dto.DateModifire,
    };
}


