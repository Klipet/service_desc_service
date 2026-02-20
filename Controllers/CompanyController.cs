using DevExpress.Xpo;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]

public class CompanyController: ControllerBase
{
    private readonly UnitOfWork _uow;
    public CompanyController(UnitOfWork uow)
    {
        _uow = uow;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        try
        {

            var category = _uow.Query<Company>().ToList();
            Console.WriteLine($"Найдено Company: {category.Count}");

            var result = category.Select(t => new CompanyDto
            {
                Oid = t.Oid,
                Name = t.Name,
                Idnp = t.Idnp,
                VipState = t.VipState,
                Active = t.Active,
                DateModifire = t.DateModified,
                DateCreated = t.DateCreated,


            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка: {ex.Message}");
        }
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var category = _uow.GetObjectByKey<Company>(id);
        if (category == null) return NotFound();

        var categoriResponse = new CompanyDto
        {
            Oid = category.Oid,
            Name = category.Name,
            Active = category.Active,
            DateCreated = category.DateCreated,
            DateModifire = category.DateModified,
            Idnp = category.Idnp,
            VipState = category.VipState,

        };
        return Ok(categoriResponse);
    }


    [HttpPost]
    public IActionResult Create([FromBody] CompanyDto model)
    {
        using var uow = MyXPO.GetNewUnitOfWork();
        if (model == null) return BadRequest("Data is null");

        var category = new Company(uow)
        {
            Name = model.Name,
            Active = model.Active,
            DateCreated = model.DateCreated,
            Idnp=model.Idnp,
            VipState = model.VipState,
        };

        try
        {
            uow.CommitChanges();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error saving to database: {ex.Message}");
        }
        var resault = new CompanyDto
        {
            Oid = category.Oid,
            Name = category.Name,
            Active = category.Active,
            DateCreated = category.DateCreated,
            Idnp = category.Idnp,
            VipState = category.VipState,
        };
        //   return CreatedAtAction("GetTiketById", new { id = ticket.Id }, resault);
        return Ok(resault);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] Company dto)
    {
        try
        {

            var category = _uow.Query<Company>().FirstOrDefault(t => t.Oid == id);

            if (category == null) return NotFound($"Тикет с Id {id} не найден");

            category.Name = dto.Name;
            category.Idnp = dto.Idnp;
            category.VipState = dto.VipState;
            category.Active = dto.Active;

            _uow.CommitChanges();

            return Ok($"Company {id} update");
        }
        catch (Exception ex)
        {
            return StatusCode(ex.HResult, $"Ошибка при обновлении: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        try
        {
            // Ищем через сессию явно
            var category = _uow.Query<Company>()
            .FirstOrDefault(t => t.Oid == id);


            if (category == null)
                return NotFound($"Тикет с id={id} не найден");

            // Удаляем
            _uow.Delete(category);
            _uow.CommitChanges();

            return NoContent(); // 204
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка при удалении: {ex.Message}");
        }
    }
}