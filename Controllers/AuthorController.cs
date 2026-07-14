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
                Phone = t.Phones.Select(p => new PhoneDtoAuthor {
                    Oid = p.Oid,
                    Number = p.Number,
                    DataCreated = p.DataCreated,
                }).ToList(),
                Email = t.Email,
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
                Email = model.Email,
            };
            if (model.Phone != null)
            {
                foreach (var phoneDto in model.Phone)
                {
                    var phone = new Phone(uow)
                    {
                        Number = phoneDto.Number,
                        Author = author,
                        DataCreated = DateTime.Now,
                    };
                }
            }

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
                Phone = author.Phones.Select(p => new PhoneDtoAuthor
                {
                    Oid = p.Oid,
                    Number = p.Number,
                    DataCreated = p.DataCreated,
                }).ToList(),
                Email = author.Email,
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
            Email = author.Email,
            Phone = author.Phones.Select(p => new PhoneDtoAuthor
            {
                Oid = p.Oid,
                Number = p.Number,
                DataCreated = p.DataCreated,
            }).ToList(),
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
            if (author == null) return NotFound($"Автор с Id {id} не найден");

            var sc = _uow.Query<Platform>().FirstOrDefault(e => e.Oid == dto.PlatformId);
            if (sc == null) throw new KeyNotFoundException("Platform не найден");

            author.Platform = sc;
            author.Name = dto.Name;
            author.Active = dto.Active;
            author.Email = dto.Email;

            // --- синхронизация телефонов ---
            var incomingIds = dto.Phone?.Where(p => p.Oid != 0)
                                         .Select(p => p.Oid)
                                         .ToHashSet() ?? new HashSet<int>();

            // удалить телефоны, которых больше нет в dto
            var toRemove = author.Phones.Where(p => !incomingIds.Contains(p.Oid)).ToList();
            foreach (var phone in toRemove)
            {
                phone.Delete();
            }

            // обновить существующие / добавить новые
            if (dto.Phone != null)
            {
                foreach (var phoneDto in dto.Phone)
                {
                    if (phoneDto.Oid != 0)
                    {
                        var existing = author.Phones.FirstOrDefault(p => p.Oid == phoneDto.Oid);
                        if (existing != null)
                        {
                            existing.Number = phoneDto.Number;
                            existing.DataModefire = DateTime.Now;
                            continue;
                        }
                    }

                    var newPhone = new Phone(_uow)
                    {
                        Number = phoneDto.Number,
                        Author = author,
                        DataCreated = DateTime.Now,
                    };
                }
            }

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


