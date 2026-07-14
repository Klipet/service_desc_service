using DevExpress.Xpo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


[ApiController]
[Route("[controller]")]
[ApiKey]
public class CompanyController : ControllerBase
{
    private readonly UnitOfWork _uow;
    public CompanyController(UnitOfWork uow)
    {
        _uow = uow;
    }

    private User CurrentUser => (User)HttpContext.Items["CurrentUser"]!;

    // Проверка права
    private bool HasPermission(string code) =>
        CurrentUser?.RoleUser?.RolePermissions
            .Any(rp => rp.Permission.Name == code
                    && rp.Permission.IsActive) ?? false;

    [HttpGet]
    public IActionResult GetAll()
    {
        if (!HasPermission(PermisionConstant.UserRead))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });

        try
        {

            var category = _uow.Query<Company>().ToList();
            
            Console.WriteLine($"Найдено Company: {category.Count}");

            var result = category.Select(t => new
            {
                Oid = t.Oid,
                Name = t.Name,
                Idnp = t.Idnp,
                CompanyStateOid = t.CompanyState.Oid,
                CompanyStateName = t.CompanyState.Name,
                Active = t.Active,
                DateModifire = t.DateModified,
                DateCreated = t.DateCreated,
                Platforms = t.Platforms.Select(p => new
                {
                    Oid = p.Oid,
                    Name = p.Name,
                    Active = p.Active,
                    DateCreated = p.DateCreated,
                    DateModifire = p.DateModified,
                    Autor = p.Authors.Select(a => new AuthorDto
                    {
                        Oid = a.Oid,
                        Name = a.Name,
                        Active = a.Active,
                        DateCreated = a.DateCreated,
                        DateModifire = a.DateModified,
                        Phone = a.Phones.Select(p => new PhoneDtoAuthor
                        {
                            Oid = p.Oid,
                            Number = p.Number,
                            DataCreated = p.DataCreated,
                        }).ToList(),
                        Email = a.Email
                    }).ToList(),
                }).ToList(),

            }).ToList();


            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка: {ex.Message}");
        }
    }

    [HttpGet("Get[controller]ById")]
    public IActionResult GetById([FromQuery] int id)
    {
        if (!HasPermission(PermisionConstant.UserRead))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });

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
            ComapnyStateOid = category.CompanyState.Oid,
            ComapnyStateName = category.CompanyState.Name,

        };
        return Ok(categoriResponse);
    }


    [HttpPost]
    public IActionResult Create([FromBody] CompanyCreateDto model)
    {
        if (!HasPermission(PermisionConstant.CompanyCreate))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });
        using var uow = MyXPO.GetNewUnitOfWork();
        if (model == null) return BadRequest("Data is null");

        var companyState = uow.Query<CompanyState>().FirstOrDefault(c => c.Oid == model.ComapnyStateOid);

        if (companyState == null) return NotFound($"CompanyState с Id {model.ComapnyStateOid} не найден");

        var company = new Company(uow)
        {
            Name = model.Name,
            Active = model.Active,
            DateCreated = DateTime.Now,
            Idnp = model.Idnp,
            CompanyState = companyState
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
            Oid = company.Oid,
            Name = company.Name,
            Active = company.Active,
            DateCreated = company.DateCreated,
            Idnp = company.Idnp,
            ComapnyStateOid = company.CompanyState.Oid,
            ComapnyStateName = company.CompanyState.Name,
        };
        //   return CreatedAtAction("GetTiketById", new { id = ticket.Id }, resault);
        return Ok(resault);
    }

    [HttpPut("Update[controller]ById")]
    public IActionResult Update([FromQuery] int id, [FromBody] CompanyDto dto)
    {
        if (!HasPermission(PermisionConstant.UserUpdate))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });
        try
        {

            var company = _uow.Query<Company>().FirstOrDefault(t => t.Oid == id);

            if (company == null) return NotFound($"Company с Id {id} не найден");
            var companyState = _uow.Query<CompanyState>().FirstOrDefault(c => c.Oid == dto.ComapnyStateOid);
            if (companyState == null) return NotFound($"CompanyState с Id {id} не найден");

            company.Name = dto.Name;
            company.Idnp = dto.Idnp;
            company.CompanyState = companyState;
            company.Active = dto.Active;

            _uow.CommitChanges();

            return Ok($"Company {id} update");
        }
        catch (Exception ex)
        {
            return StatusCode(ex.HResult, $"Ошибка при обновлении: {ex.Message}");
        }
    }

    [HttpDelete("Delete[controller]ById")]
    public IActionResult Delete([FromQuery] int id)
    {
        if (!HasPermission(PermisionConstant.UserDelete))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });
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

    [HttpGet("Search")]
    public IActionResult Search([FromQuery] string? query, [FromQuery] int take = 20)
    {
        if (!HasPermission(PermisionConstant.UserRead))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });

        try
        {
            var queryable = _uow.Query<Company>().Where(c => c.Active);

            if (!string.IsNullOrWhiteSpace(query))
            {
                var lowerQuery = query.ToLower();
                queryable = queryable.Where(c => c.Name.ToLower().Contains(lowerQuery));
            }

            var result = queryable.Select(t => new
            {
                Oid = t.Oid,
                Name = t.Name,
                Idnp = t.Idnp,
                CompanyStateOid = t.CompanyState.Oid,
                CompanyStateName = t.CompanyState.Name,
                Active = t.Active,
                DateModifire = t.DateModified,
                DateCreated = t.DateCreated,
                Platforms = t.Platforms.Select(p => new
                {
                    Oid = p.Oid,
                    Name = p.Name,
                    Active = p.Active,
                    DateCreated = p.DateCreated,
                    DateModifire = p.DateModified,
                    Autor = p.Authors.Select(a => new AuthorDto
                    {
                        Oid = a.Oid,
                        Name = a.Name,
                        Active = a.Active,
                        DateCreated = a.DateCreated,
                        DateModifire = a.DateModified,
                        Phone = a.Phones.Select(p => new PhoneDtoAuthor
                        {
                            Oid = p.Oid,
                            Number = p.Number,
                            DataCreated = p.DataCreated,
                        }).ToList(),
                        Email = a.Email
                    }).ToList(),
                }).ToList(),

            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
         //   _logger.LogError(ex, "Ошибка при поиске компаний по запросу {Query}", query);
            return StatusCode(500, new { error = "Внутренняя ошибка сервера", errorCode = 500 });
        }
    }
}