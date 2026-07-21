using DevExpress.Xpo;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("[controller]")]
[ApiKey]
public class TotalTypeController : ControllerBase
{
    private readonly UnitOfWork _uow;
    public TotalTypeController(UnitOfWork uow)
    {
        _uow = uow;
    }
    private User CurrentUser => (User)HttpContext.Items["CurrentUser"]!;
    // Проверка права
    private bool HasPermission(string code) =>
        CurrentUser?.RoleUser?.RolePermissions
            .Any(rp => rp.Permission.Name == code
                    && rp.Permission.IsActive) ?? false;


    [HttpGet()]
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
            .ToList()
            .Select(t => new
            {
                Oid = t.Oid,
                Name = t.Name,
                Active = t.Active,
                DateCreated = t.DateCreated,
                SubCategories = t.SubCategorys
                .Where(c => c.Active)
                .Select( c => new TotalTypeDto
                 {
                    Oid = c.Oid,
                    Name = c.Name,
                    Active = c.Active,
                    DateCreated = c.DateCreated,
                }).ToList()
            }).ToList();

        var workSpaces = _uow.Query<WorkSpace>()
            .Where(t => t.Active)
            .ToList();

        var tiketWorkSpace = workSpaces
            .Select(t => new 
            {
                Oid = t.Oid,
                Name = t.Name,
                Active = t.Active,
                DateCreated = t.DateCreated,
                User = t.Users
                    .Where(uw => uw.IsActive)
                    .Select(uw => new TotalTypeDto
                    {
                        Oid = uw.Oid,
                        Name = uw.Name,
                        Active = uw.IsActive,
                        DateCreated = uw.DateCreated,
                    }).ToList()
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