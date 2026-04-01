using DevExpress.Xpo;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
[ApiKey]
public class AuthorController : ControllerBase
{
    private readonly UnitOfWork _uow;
    public AuthorController(UnitOfWork uow)
    {
        _uow = uow;
    }

    private User CurrentUser => (User)HttpContext.Items["CurrentUser"]!;

    // Проверка права
    private bool HasPermission(string code) =>
        CurrentUser?.RoleUser?.RolePermissions
            .Any(rp => rp.Permission.Name == code
                    && rp.Permission.IsActive) ?? false;

    [HttpGet("GetAll[controller]")]
    public IActionResult GetAll()
    {

        if (!HasPermission(PermisionConstant.AuthorRead))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });
        try
        {

            var category = _uow.Query<Author>().ToList();
            Console.WriteLine($"Найдено Author: {category.Count}");

            var result = category.Select(t => new AuthorDto
            {
                Oid = t.Oid,
                Name = t.Name,
                Active = t.Active,
                phone = t.Phone,
                email = t.Email,
                DateCreated = t.DateCreated,
                DateModifire = t.DateModified,

            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка: {ex.Message}");
        }
    }

    [HttpPost("Create[controller]")]
    public IActionResult Create([FromBody] AuthorDto model)
    {
        if (!HasPermission(PermisionConstant.AuthorCreate))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });
        try
        {
            using var uow = MyXPO.GetNewUnitOfWork();
            if (model == null) return BadRequest("Data is null");

            var pl = uow.Query<Platform>().FirstOrDefault(e => e.Oid == model.PlatformId);
            if (pl == null) throw new KeyNotFoundException("SubCategory не найден");

            var author = new Author(uow)
            {
                Name = model.Name,
                Active = model.Active,
                DateCreated = model.DateCreated,
                Platform = pl,
                Phone = model.phone,
                Email = model.email,
            };

            try
            {
                uow.CommitChanges();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error saving to database: {ex.Message}");
            }
            var resault = new AuthorDto
            {
                Oid = author.Oid,
                Name = author.Name,
                Active = author.Active,
                DateCreated = author.DateCreated,
                PlatformId = author.Platform.Oid,
                PlatformName = author.Platform.Name,
                phone = author.Phone,
                email = author.Email,
            };
            //   return CreatedAtAction("GetTiketById", new { id = ticket.Id }, resault);
            return Ok(resault);
        }
        catch (Exception ex)
        {
            return StatusCode(ex.HResult, $"ошибка сохранения: {ex.Message}");
        }
    }


    [HttpGet("GetAutorById")]
    public IActionResult GetById([FromQuery] int id)
    {
        if (!HasPermission(PermisionConstant.AuthorRead))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });

        var author = _uow.GetObjectByKey<Author>(id);
        if (author == null) return NotFound();

        var pl = _uow.GetObjectByKey<Platform>(author.Platform) ?? throw new KeyNotFoundException("точка продажи не найдена");

        var authorResponse = new AuthorDto
        {
            Oid = author.Oid,
            Name = author.Name,
            Active = author.Active,
            DateCreated = author.DateCreated,
            DateModifire = author.DateModified,
            email = author.Email,
            phone = author.Phone,
            PlatformId = pl.Oid,
            PlatformName = pl.Name
        };
        return Ok(authorResponse);
    }

    [HttpPut("UpdateAutorById")]
    public IActionResult Update([FromQuery] int id, [FromBody] AuthorDto dto)
    {

        if (!HasPermission(PermisionConstant.AuthorUpdate))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });
        try
        {
            var author = _uow.Query<Author>().FirstOrDefault(t => t.Oid == id);

            if (author == null) return NotFound($"Тикет с Id {id} не найден");

            var sc = _uow.Query<Platform>().FirstOrDefault(e => e.Oid == dto.PlatformId);
            if (sc == null) throw new KeyNotFoundException("Platform не найден");

            author.Platform = sc;
            author.Name = dto.Name;
            author.Active = dto.Active;
            author.Phone = dto.phone;
            author.Email = dto.email;

            _uow.CommitChanges();

            return Ok($"Author {id} update");
        }
        catch (Exception ex)
        {
            return StatusCode(ex.HResult, $"Ошибка при обновлении: {ex.Message}");
        }
    }

    [HttpDelete("DeleteAutor")]
    public IActionResult Delete([FromQuery] int id)
    {
        if (!HasPermission(PermisionConstant.AuthorDelete))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });
        try
        {
            // Ищем через сессию явно
            var author = _uow.Query<Author>()
            .FirstOrDefault(t => t.Oid == id);


            if (author == null) return NotFound($"Author с id={id} не найден");

            // Удаляем
            _uow.Delete(author);
            _uow.CommitChanges();

            return NoContent(); // 204
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка при удалении: {ex.Message}");
        }
    }
}


