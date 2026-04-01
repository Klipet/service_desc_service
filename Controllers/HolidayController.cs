using DevExpress.Xpo;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiKey]
[Route("Settings/[controller]")]
public class HolidayController : ControllerBase
{
    private readonly UnitOfWork _uow;
    public HolidayController(UnitOfWork uow) => _uow = uow;

    private User CurrentUser => (User)HttpContext.Items["CurrentUser"]!;

    // Проверка права
    private bool HasPermission(string code) =>
        CurrentUser?.RoleUser?.RolePermissions
            .Any(rp => rp.Permission.Name == code
                    && rp.Permission.IsActive) ?? false;

    [HttpGet]
    public IActionResult GetAll()
    {
        if (!HasPermission(PermisionConstant.HolidayRead))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });
        var list = new XPCollection<HolidayDay>(_uow)
            .Select(h => new
            {
                h.Oid,
                h.Name,
                Date = h.Date.ToString("dd.MM.yyyy"),
                h.IsRecurringYearly
            }).ToList();
        return Ok(list);
    }

    [HttpPost]
    public IActionResult Create([FromBody] HolidayDto dto)
    {
        if (!HasPermission(PermisionConstant.HolidayCreate))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });
        var h = new HolidayDay(_uow)
        {
            Name = dto.Name,
            Date = dto.Date.Date,
            IsRecurringYearly = dto.IsRecurringYearly
        };
        _uow.CommitChanges();
        return Ok(new { h.Oid, h.Name, h.Date, h.IsRecurringYearly });
    }

    [HttpDelete("Delete[controller]")]
    public IActionResult Delete([FromQuery]int id)
    {
        if (!HasPermission(PermisionConstant.HolidayDelete))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });

        var h = _uow.GetObjectByKey<HolidayDay>(id);
        if (h == null) return NotFound();
        _uow.Delete(h);
        _uow.CommitChanges();
        return Ok();
    }
}