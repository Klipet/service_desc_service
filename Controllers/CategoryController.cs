using DevExpress.Xpo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static DevExpress.Data.Helpers.ExpressiveSortInfo;

[ApiController]
[Route("api/[controller]")]
public class CategoryController: ControllerBase
{
    private readonly UnitOfWork _uow;
    public CategoryController(UnitOfWork uow)
    {
        _uow = uow;
    }


    [HttpGet]
    public IActionResult GetAll()
    {
        try
        {

            var category = _uow.Query<Category>().ToList();
            Console.WriteLine($"Найдено тикетов: {category.Count}");
            

            var result = category.Select(t => new CategoryDto
            {
                Oid = t.Oid,
                Name = t.Name,
                Active = t.Active,
                subCategoryId = t.SubCategory.Oid,
                subCategoryName = t.SubCategory.Name,
                DateCreated = t.DateCreated,
                
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
        using var uow = MyXPO.GetNewUnitOfWork();
        if (model == null) return BadRequest("Data is null");

        var sc = uow.Query<SubCategory>().FirstOrDefault(e => e.Oid == model.subCategoryId);
        if (sc == null) throw new KeyNotFoundException("SubCategory не найден");

        var category = new Category(uow)
        {
            Name = model.Name,
            Active = model.Active,
            DateCreated = model.DateCreated,
            SubCategory = sc,
        };

        try
        {
            uow.CommitChanges();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error saving to database: {ex.Message}");
        }
        var resault = new CategoryDto
        {
            Oid = category.Oid,
            Name = category.Name,
            Active = category.Active,
            DateCreated = category.DateCreated,
            subCategoryName = sc.Name,
            subCategoryId = sc.Oid
        };
        //   return CreatedAtAction("GetTiketById", new { id = ticket.Id }, resault);
        return Ok(resault);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var category = _uow.GetObjectByKey<Category>(id);
        if (category == null) return NotFound();

        var sc = _uow.GetObjectByKey<SubCategory>(category.SubCategory) ?? throw new KeyNotFoundException("SubCategory не найден");
        
        var categoriResponse = new CategoryDto
        {
            Oid = category.Oid,
            Name = category.Name,
            Active = category.Active,
            DateCreated = category.DateCreated,
            subCategoryName = sc.Name,
            subCategoryId = sc.Oid
        };
        return Ok(categoriResponse);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] CategoryDto dto)
    {
        try
        {

            var category = _uow.Query<Category>().FirstOrDefault(t => t.Oid == id);

            if (category == null)
                return NotFound($"Тикет с Id {id} не найден");

            var sc = _uow.Query<SubCategory>().FirstOrDefault(e => e.Oid == dto.subCategoryId);
            if (sc == null) throw new KeyNotFoundException("SubCategory не найден");

            category.SubCategory = sc;
            category.Name = dto.Name;
            category.Active = dto.Active;

            _uow.CommitChanges();

            return Ok($"Category {id} update");
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
