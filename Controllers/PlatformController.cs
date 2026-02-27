using DevExpress.Xpo;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class PlatformController: ControllerBase
{
    private readonly UnitOfWork _uow;
    public PlatformController(UnitOfWork uow)
    {_uow = uow;}


    [HttpGet("GetAll[controller]")]
    public IActionResult GetAll()
    {
        try
        {

            var platform = _uow.Query<Platform>().ToList();
            Console.WriteLine($"Найдено Company: {platform.Count}");

            var result = platform.Select(t => new PlatformDto
            {
                Oid = t.Oid,
                Name = t.Name,
                Active = t.Active,
                DateCreated = t.DateCreated,
                companyId = t.Company.Oid,
                companyName = t.Company.Name,
                DateModifire = t.DateModified,
         
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка: {ex.Message}");
        }
    }

    [HttpPost("New[controller]")]
    public IActionResult Create([FromBody] PlatformDto model)
    {
        using var uow = MyXPO.GetNewUnitOfWork();
        if (model == null) return BadRequest("Data is null");

        var sc = uow.Query<Company>().FirstOrDefault(e => e.Oid == model.companyId);
        if (sc == null) throw new KeyNotFoundException("Company не найден");

        var category = new Platform(uow)
        {
            Name = model.Name,
            Active = model.Active,
            DateCreated = model.DateCreated,
            Company = sc,
         
        };

        try
        {
            uow.CommitChanges();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error saving to database: {ex.Message}");
        }
        var resault = new PlatformDto
        {
            Oid = category.Oid,
            Name = category.Name,
            Active = category.Active,
            DateCreated = category.DateCreated,
            companyId = sc.Oid,
            companyName = sc.Name
        };
        //   return CreatedAtAction("GetTiketById", new { id = ticket.Id }, resault);
        return Ok(resault);
    }

    [HttpGet("Get[controller]ById")]
    public IActionResult GetById([FromQuery] int id)
    {
        var category = _uow.GetObjectByKey<Platform>(id);
        if (category == null) return NotFound();

        var sc = _uow.GetObjectByKey<Company>(category.Oid) ?? throw new KeyNotFoundException("Company не найден");

        var categoriResponse = new PlatformDto
        {
            Oid = category.Oid,
            Name = category.Name,
            Active = category.Active,
            DateCreated = category.DateCreated,
            companyId=sc.Oid,
            companyName = sc.Name,
            DateModifire = category.DateModified
        };
        return Ok(categoriResponse);
    }

    [HttpPut("Update[controller]ById")]
    public IActionResult Update([FromQuery] int id, [FromBody] PlatformDto dto)
    {
        try
        {

            var platform = _uow.Query<Platform>().FirstOrDefault(t => t.Oid == id);

            if (platform == null) return NotFound($"Рабочее место с Id {id} не найден");

            var sc = _uow.Query<Company>().FirstOrDefault(e => e.Oid == dto.companyId);
            if (sc == null) throw new KeyNotFoundException("Компания не найден");

            platform.Company = sc;
            platform.Name = dto.Name;
            platform.Active = dto.Active;

            _uow.CommitChanges();

            return Ok($"platform {id} update");
        }
        catch (Exception ex)
        {
            return StatusCode(ex.HResult, $"Ошибка при обновлении: {ex.Message}");
        }
    }

    [HttpDelete("Delete[controller]")]
    public IActionResult Delete([FromQuery] int id)
    {
        try
        {
            // Ищем через сессию явно
            var platform = _uow.Query<Platform>().FirstOrDefault(t => t.Oid == id);

            if (platform == null)
                return NotFound($"Тикет с id={id} не найден");

            // Удаляем
            _uow.Delete(platform);
            _uow.CommitChanges();

            return NoContent(); // 204
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка при удалении: {ex.Message}");
        }
    }
}