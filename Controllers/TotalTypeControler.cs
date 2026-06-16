using Microsoft.AspNetCore.Mvc;
using DevExpress.Xpo;

[ApiController]
[Route("[controller]")]
[ApiKey]
public class TotalTypeControler : ControllerBase
{
    private readonly UnitOfWork _uow;
    public TotalTypeControler(UnitOfWork uow)
    {
        _uow = uow;
    }
    private User CurrentUser => (User)HttpContext.Items["CurrentUser"]!;
    // Проверка права
    private bool HasPermission(string code) =>
        CurrentUser?.RoleUser?.RolePermissions
            .Any(rp => rp.Permission.Name == code
                    && rp.Permission.IsActive) ?? false;


    [HttpGet("TotalType")]
    public IActionResult GetAll()
    {
        if (!HasPermission(PermisionConstant.TicketCreate))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });
        var tiketType = _uow.Query<TiketType>()
            .Where(t => t.Active)
            .Select(t => new TotalTypeDto
            {
                Oid = t.Oid,
                Name = t.Name,
                Active = t.Active,
                DateCreated = t.DateCreated,
            }).ToList();

        var tiketState = _uow.Query<State>()
            .Where(t => t.Active)
            .Select(t => new TotalTypeDto
            {
                Oid = t.Oid,
                Name = t.Name,
                Active = t.Active,
                DateCreated = t.DateCreated,
            }).ToList();

        var tiketPreority = _uow.Query<Preority>()
            .Where(t => t.Active)
            .Select(t => new TotalTypeDto{
                Oid = t.Oid,
                Name = t.Name,
                Active = t.Active,
                DateCreated = t.DateCreated,
            }).ToList();

        var tiketMode = _uow.Query<Mode>()
            .Where(t => t.Active)
            .Select(t => new TotalTypeDto
            {
                Oid = t.Oid,
                Name = t.Name,
                Active = t.Active,
                DateCreated = t.DateCreated,
            }).ToList();
        var tiketCategory = _uow.Query<Category>()
            .Where(t => t.Active)
            .Select(t => new TotalTypeDto
            {
                Oid = t.Oid,
                Name = t.Name,
                Active = t.Active,
                DateCreated = t.DateCreated,
            }).ToList();
        var tiketWorkSpace = _uow.Query<WorkSpace>()
            .Where(t => t.Active)
            .Select(t => new TotalTypeDto
            {
                Oid = t.Oid,
                Name = t.Name,
                Active = t.Active,
                DateCreated = t.DateCreated,
            }).ToList();

        return Ok( new{
            tiketType,
            tiketState,
            tiketPreority,
            tiketMode,
            tiketCategory,
            tiketWorkSpace
        });
    }

}