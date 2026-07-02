using DevExpress.Xpo;
using Microsoft.AspNetCore.Mvc;
using static DevExpress.Data.Helpers.ExpressiveSortInfo;


[ApiController]
[ApiKey]
[Route("[controller]")]

public class SubCategoryController: ControllerBase
{
    private readonly UnitOfWork _uow;
    public SubCategoryController(UnitOfWork uow)
    { _uow = uow; }

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
            var result = _uow.Query<SubCategory>()
                .Select(t => new
                {
                    Oid = t.Oid,
                    Name = t.Name,
                    Active = t.Active,
                    DateCreated = t.DateCreated,

                    Category = t.Category != null && t.Category.Active
                        ? new
                        {
                            Oid = t.Category.Oid,
                            Name = t.Category.Name,
                            Active = t.Category.Active
                        }
                        : null
                })
                .ToList();

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
        if (!HasPermission(PermisionConstant.CategoryRead))
            return StatusCode(403);

        var subCategory = _uow.GetObjectByKey<SubCategory>(id);

        if (subCategory == null)
            return NotFound();

        return Ok(new
        {
            subCategory.Oid,
            subCategory.Name,
            subCategory.Active,
            subCategory.DateCreated,
            Category = subCategory.Category == null ? null : new
            {
                subCategory.Category.Oid,
                subCategory.Category.Name
            }
        });
    }

    [HttpPost]
    public IActionResult Create([FromBody] SubCategoryDto dto)
    {
        if (!HasPermission(PermisionConstant.CategoryCreate))
            return StatusCode(403);

        var category = _uow.GetObjectByKey<Category>(dto.CategoryOid);

        if (category == null)
            return BadRequest("Категория не найдена.");

        var subCategory = new SubCategory(_uow)
        {
            Name = dto.Name,
            Active = dto.Active,
            Category = category
        };

        _uow.CommitChanges();

        return Ok(subCategory.Oid);
    }

    [HttpPut("Update[controller]ById")]
    public IActionResult Update(int id, [FromBody] SubCategoryDto dto)
    {
        if (!HasPermission(PermisionConstant.CategoryUpdate))
            return StatusCode(403);

        var subCategory = _uow.GetObjectByKey<SubCategory>(id);

        if (subCategory == null)
            return NotFound();

        var category = _uow.GetObjectByKey<Category>(dto.CategoryOid);

        if (category == null)
            return BadRequest("Категория не найдена.");

        subCategory.Name = dto.Name;
        subCategory.Active = dto.Active;
        subCategory.Category = category;
        subCategory.DateModified = DateTime.Now;

        _uow.CommitChanges();

        return Ok();
    }

    [HttpDelete("Delete[controller]ById")]
    public IActionResult Delete(int id)
    {
        if (!HasPermission(PermisionConstant.CategoryDelete))
            return StatusCode(403);

        var subCategory = _uow.GetObjectByKey<SubCategory>(id);

        if (subCategory == null)
            return NotFound();

        subCategory.Delete();

        _uow.CommitChanges();

        return Ok();
    }
}

