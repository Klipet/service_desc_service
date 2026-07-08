using DevExpress.Data.Extensions;
using DevExpress.Xpo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static DevExpress.Data.Helpers.ExpressiveSortInfo;

[ApiController]
[Route("[controller]")]
[ApiKey]
public class CategoryController : ControllerBase
{
    private readonly UnitOfWork _uow;
    public CategoryController(UnitOfWork uow)
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
        if (!HasPermission(PermisionConstant.CategoryRead))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });
        try
        {

            var category = _uow.Query<Category>().ToList();
            Console.WriteLine($"Найдено тикетов: {category.Count}");


            var result = category.Select(t => new
            {
                Oid = t.Oid,
                Name = t.Name,
                Active = t.Active,
                DateCreated = t.DateCreated,
                subCategory = t.SubCategorys
                .Where(sc => sc.Active)
                .Select(sc => new{
                   sc.Oid,
                   sc.Name,
                   sc.Active
                  }
                ).ToList(),
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка: {ex.Message}");
        }
    }


    [HttpPost]
    public IActionResult Create([FromBody] CategoryDto model)
    {
        if (!HasPermission(PermisionConstant.CategoryCreate))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });


        using var uow = MyXPO.GetNewUnitOfWork();
        if (model == null) return BadRequest("Data is null");


        var category = new Category(uow)
        {
            Name = model.Name,
            Active = model.Active,
            DateCreated = model.DateCreated,
        };

        try
        {
            uow.CommitChanges();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error saving to database: {ex.Message}");
        }
        var resault = new 
        {
            Oid = category.Oid,
            Name = category.Name,
            Active = category.Active,
            DateCreated = category.DateCreated,
        };
        return Ok(resault);
    }

    [HttpGet("Get[controller]ById")]
    public IActionResult GetById([FromQuery] int id)
    {
        if (!HasPermission(PermisionConstant.CategoryRead))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });

        var category = _uow.GetObjectByKey<Category>(id);
        if (category == null) return NotFound();

    //    var sc = _uow.GetObjectByKey<SubCategory>(category.SubCategory) ?? throw new KeyNotFoundException("SubCategory не найден");

        var categoriResponse = new CategoryDto
        {
            Oid = category.Oid,
            Name = category.Name,
            Active = category.Active,
            DateCreated = category.DateCreated,
        };
        return Ok(categoriResponse);
    }

    [HttpPut("Update[controller]ById")]
    public IActionResult Update(int id, [FromBody] CategoryDto dto)
    {

        if (!HasPermission(PermisionConstant.CategoryUpdate))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });
        try
        {

            var category = _uow.Query<Category>().FirstOrDefault(t => t.Oid == id);

            if (category == null)
                return NotFound($"Тикет с Id {id} не найден");


            category.Name = dto.Name;
            category.Active = dto.Active;
            category.DateModified = DateTime.Now;
            _uow.CommitChanges();

            return Ok($"Category {id} update");
        }
        catch (Exception ex)
        {
            return StatusCode(ex.HResult, $"Ошибка при обновлении: {ex.Message}");
        }
    }

    [HttpDelete("Delete[controller]ById")]
    public IActionResult Delete(int id)
    {
        if (!HasPermission(PermisionConstant.CategoryDelete))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });
        try
        {
            // Ищем через сессию явно
            var category = _uow.Query<Category>()
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
