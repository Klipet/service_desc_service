using DevExpress.Xpo;
using static DevExpress.Data.Helpers.ExpressiveSortInfo;

public class SubCategoryController: BaseController<SubCategory, SubCategoryDto>
{
    public SubCategoryController(UnitOfWork uow) : base(uow) { }
    protected override string PermissionRead => PermisionConstant.SubCategoryRead;
    protected override string PermissionCreate => PermisionConstant.SubCategoryCreate;
    protected override string PermissionUpdate => PermisionConstant.SubCategoryUpdate;
    protected override string PermissionDelete => PermisionConstant.SubCategoryDelete;
    protected override SubCategory CreateModel(SubCategoryDto dto) => new SubCategory(_uow)
{
    Name = dto.Name,
    Active = dto.Active,
    DateCreated = DateTime.UtcNow,
    DateModified = DateTime.UtcNow,
};
}

