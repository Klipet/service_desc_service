using DevExpress.Xpo;
using static DevExpress.Data.Helpers.ExpressiveSortInfo;

public class SubCategoryController: BaseController<SubCategory, SubCategoryDto>
{
    public SubCategoryController(UnitOfWork uow) : base(uow) { }
protected override SubCategory CreateModel(SubCategoryDto dto) => new SubCategory(_uow)
{
    Name = dto.Name,
    Active = dto.Active,
    DateCreated = DateTime.UtcNow,
    DateModified = DateTime.UtcNow,
};
}

