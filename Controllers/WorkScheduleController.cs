using DevExpress.Xpo;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class WorkScheduleController : ControllerBase
{
    private readonly UnitOfWork _uow;
    public WorkScheduleController(UnitOfWork uow) => _uow = uow;

    [HttpGet]
    public IActionResult GetAll()
    {
        var list = new XPCollection<WorkSchedule>(_uow)
            .OrderBy(s => s.DayOfWeek)
            .Select(s => new
            {
                s.Oid,
                s.DayName,
                s.DayOfWeek,
                StartTime = s.StartTime.ToString(@"hh\:mm"),
                EndTime = s.EndTime.ToString(@"hh\:mm"),
                s.IsWorkingDay
            }).ToList();
        return Ok(list);
    }

    [HttpPut("Update[controller]")]
    public IActionResult Update([FromQuery] int id, [FromBody] WorkScheduleDto dto)
    {
        var s = _uow.GetObjectByKey<WorkSchedule>(id);
        if (s == null) return NotFound();

        s.StartTime = dto.StartTime;
        s.EndTime = dto.EndTime;
        s.IsWorkingDay = dto.IsWorkingDay;

        _uow.CommitChanges();
        return Ok();
    }
}
