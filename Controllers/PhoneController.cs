
using DevExpress.Xpo;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
[ApiKey]
public class PhoneController : ControllerBase
{
    private readonly UnitOfWork _uow;
    public PhoneController(UnitOfWork uow)
    {
        _uow = uow;
    }
    private User CurrentUser => (User)HttpContext.Items["CurrentUser"]!;

    private bool HasPermission(string code) =>
      CurrentUser?.RoleUser?.RolePermissions
          .Any(rp => rp.Permission.Name == code
                  && rp.Permission.IsActive) ?? false;


    [HttpGet("GetAllPhones")]
    public IActionResult GetAll()
    {
        if (!HasPermission(PermisionConstant.AuthorRead))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });
        try
        {
            var result = _uow.Query<Phone>().Select(p => new PhoneDto
            {
                Oid = p.Oid,
                Number = p.Number,
                DataCreated = p.DataCreated,
                DataModefire = p.DataModefire,
                AuthorId = p.Author != null ? p.Author.Oid : 0,
                AuthorName = p.Author != null ? p.Author.Name : string.Empty,
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка при получении списка: {ex.Message}");
        }
    }

    // POST: api/Phone/CreatePhone?authorId=1
    [HttpPost("CreatePhone")]
    public IActionResult Create([FromBody] PhoneDto model)
    {
        if (!HasPermission(PermisionConstant.AuthorCreate))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });
        try
        {
            if (model == null) return BadRequest("Data is null");

            var author = _uow.Query<Author>().FirstOrDefault(a => a.Oid == model.AuthorId);
            if (author == null) throw new KeyNotFoundException("Author не найден");

            var phone = new Phone(_uow)
            {
                Number = model.Number,
                Author = author,
                DataCreated = DateTime.Now,
            };

            try
            {
                _uow.CommitChanges();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error saving to database: {ex.Message}");
            }

            var result = new PhoneDto
            {
                Oid = phone.Oid,
                Number = phone.Number,
                DataCreated = phone.DataCreated,
                AuthorId = phone.Author != null ? phone.Author.Oid : 0,
                AuthorName = phone.Author != null ? phone.Author.Name : string.Empty,
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(ex.HResult, $"ошибка сохранения: {ex.Message}");
        }
    }

    [HttpPut("UpdatePhoneById")]
    public IActionResult Update([FromBody] PhoneDto dto)
    {
        if (!HasPermission(PermisionConstant.AuthorUpdate))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });
        try
        {
            var phone = _uow.Query<Phone>().FirstOrDefault(p => p.Oid == dto.Oid);
            if (phone == null) return NotFound($"Phone с id={dto.Oid} не найден");

            if (dto.AuthorId != 0 && dto.AuthorId != phone.Author?.Oid)
            {
                var author = _uow.Query<Author>().FirstOrDefault(a => a.Oid == dto.AuthorId);
                if (author == null) throw new KeyNotFoundException("Author не найден");
                phone.Author = author;
            }

            phone.Number = dto.Number;
            phone.DataModefire = DateTime.Now;

            _uow.CommitChanges();
            return Ok($"Phone {phone.Oid} update");
        }
        catch (Exception ex)
        {
            return StatusCode(ex.HResult, $"Ошибка при обновлении: {ex.Message}");
        }
    }

    // DELETE: api/Phone/DeletePhone?id=1
    [HttpDelete("DeletePhone")]
    public IActionResult Delete([FromQuery] int id)
    {
        if (!HasPermission(PermisionConstant.AuthorDelete))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });
        try
        {
            var phone = _uow.Query<Phone>().FirstOrDefault(p => p.Oid == id);
            if (phone == null) return NotFound($"Phone с id={id} не найден");

            phone.Delete();
            _uow.CommitChanges();
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка при удалении: {ex.Message}");
        }
    }

}

