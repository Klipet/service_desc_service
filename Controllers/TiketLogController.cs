using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
[ApiKey]
public class TiketLogController: ControllerBase
{
    private readonly UnitOfWork _uow;
    public TiketLogController(UnitOfWork uow) { _uow = uow; }
    private User CurrentUser => (User)HttpContext.Items["CurrentUser"]!;

    // Проверка права
    private bool HasPermission(string code) =>
        CurrentUser?.RoleUser?.RolePermissions
            .Any(rp => rp.Permission.Name == code
                    && rp.Permission.IsActive) ?? false;

    [HttpGet]
    public IActionResult GetAll()
    {
        if (!HasPermission(PermisionConstant.TicketRead))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });

        try
        { 
            var tikets = _uow.Query<TiketLog>().ToList();
            Console.WriteLine($"Найдено изменений: {tikets.Count}");

            var result = tikets.Select(t => new TiketLogDto
            {
                TiketId = t.Oid,
                Action = t.Action,
                UserName = t.User.Name,
                Title = t.Title,
                Description = t.Description,
                CompanyName = t.Company.Name,
                CategoryName = t.Category.Name,
                SubCategoryName = t.SubCategory.Name,
                StateName = t.State.Name,
                TypeTiketName = t.TypeTiket.Name,
                AuthorName = t.Author.Name,
                PlatformName = t.Platform.Name,
                WorkSpaceName = t.WorkSpace.Name,
                PreorityName = t.Preority.Name,
                Phone = t.Phone,
                DataPhone = t.DataPhone,
                ResaultPhone = t.ResaultPhone,
                DateSecondPhone = t.DateSecondPhone,
                BugNumber = t.BugNumber,
                ModeName = t.Mode.Name,
                BugTransfer = t.BugTransfer,
                DataModefire = t.DataModefire,
            }).ToList();
             return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка: {ex.Message}");
        }
    }
    [HttpGet("Get[controller]ById")]
    public IActionResult GetByTiketId([FromQuery]int tiketId)
    {
        if (!HasPermission(PermisionConstant.TicketRead))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });

        var tiketLog = _uow.Query<TiketLog>().Where(t => t.TiketId == tiketId).ToList();
        if (!tiketLog.Any()) return NotFound($"Лог для тикета с Id {tiketId} не найден");

        var result = tiketLog.Select(t => new TiketLogDto
        {
            TiketId = t.Oid,
            Action = t.Action,
            UserName = t.User.Name,

            Title = t.Title,
            Description = t.Description,
            CompanyName = t.Company.Name,
            CategoryName = t.Category.Name,
            SubCategoryName = t.SubCategory.Name,
            StateName = t.State.Name,
            TypeTiketName = t.TypeTiket.Name,
            AuthorName = t.Author.Name,
            PlatformName = t.Platform.Name,
            WorkSpaceName = t.WorkSpace.Name,
            PreorityName = t.Preority.Name,
            Phone = t.Phone,
            DataPhone = t.DataPhone,
            ResaultPhone = t.ResaultPhone,
            DateSecondPhone = t.DateSecondPhone,
            BugNumber = t.BugNumber,
            ModeName = t.Mode.Name,
            BugTransfer = t.BugTransfer,
            DataModefire = t.DataModefire,

        }).ToList();
        return Ok(result);
    }

}


