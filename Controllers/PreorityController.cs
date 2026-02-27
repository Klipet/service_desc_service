using DevExpress.Xpo;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("Settings/[controller]")]
public class PreorityController : ControllerBase
{
    private readonly UnitOfWork _uow;
    public PreorityController(UnitOfWork uow) => _uow = uow;

    [HttpGet]
    public IActionResult GetAll()
    {
        var list = new XPCollection<Preority>(_uow)
            .Select(p => new
            {
                p.Oid,
                p.Name,
                p.DeadlineHours,
                p.DeadlineLabel,
                p.Active,
                p.DateCreated,
            }).ToList();
        return Ok(list);
    }

    [HttpPut("UpdateById")]
    public IActionResult Update([FromQuery] int id, [FromBody] PreorityDto dto)
    {
        var p = _uow.GetObjectByKey<Preority>(id);
        if (p == null) return NotFound();
        p.Name = dto.Name;
        p.Active = dto.Active;
        p.DeadlineHours = dto.DeadlineHours;
        _uow.CommitChanges();
        return Ok();
    }
    [HttpDelete("Delete[controller]ById")]
    public IActionResult Delete([FromQuery] int id)
    {
        try
        {
            // Ищем через сессию явно
            var preority = _uow.Query<Preority>()
            .FirstOrDefault(t => t.Oid == id);


            if (preority == null)
                return NotFound($"Тикет с id={id} не найден");

            // Удаляем
            _uow.Delete(preority);
            _uow.CommitChanges();

            return NoContent(); // 204
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка при удалении: {ex.Message}");
        }
    }

    [HttpGet("GetTicketById")]
    public IActionResult GetById([FromQuery] int id)
    {
        var preority = _uow.GetObjectByKey<Preority>(id);
        if (preority == null) return NotFound();

        var tiketDto = new
        {
            Oid = preority.Oid,
            Name = preority.Name,
            Active = preority.Active,
            DeadlineHours = preority.DeadlineHours,
            DateCreated = preority.DateCreated,
            DeadlineLabel = preority.DeadlineLabel,
        };

        return Ok(tiketDto);

    }

   }


