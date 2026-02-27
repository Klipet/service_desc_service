using DevExpress.Xpo;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public abstract class BaseController<TModel, TDto> : ControllerBase
    where TModel : BaseEntity
    where TDto : BaseDto, new()
{
    protected readonly UnitOfWork _uow;

    protected BaseController(UnitOfWork uow)
    {
        _uow = uow;
    }

    // Маппинг — переопределяй в дочернем если нужно
    protected virtual TDto ToDto(TModel model) => new TDto
    {
        Oid = model.Oid,
        Name = model.Name,
        Active = model.Active,
        DateCreated = model.DateCreated,
    };

    protected abstract TModel CreateModel(TDto dto);

    [HttpGet("All[controller]")]
    public IActionResult GetAll()
    {
        try
        {
            var items = _uow.Query<TModel>().ToList();
            return Ok(items.Select(ToDto));
        }
        catch (Exception ex) { return StatusCode(500, ex.Message); }
    }

    [HttpGet("Get[controller]ById")]
    public IActionResult GetById([FromQuery] int id)
    {
        try
        {
            var item = _uow.GetObjectByKey<TModel>(id);
            if (item == null) return NotFound();
            return Ok(ToDto(item));
        }
        catch (Exception ex) { return StatusCode(500, ex.Message); }
    }

    [HttpPost("New[controller]")]
    public IActionResult Create([FromBody] TDto dto)
    {
        try
        {
            if (dto == null) return BadRequest("Model is null");
            var model = CreateModel(dto);
            _uow.CommitChanges();
            return Ok(ToDto(model));
        }
        catch (Exception ex) { return StatusCode(500, ex.Message); }
    }

    [HttpPut("Upadate[controller]ById")]
    public IActionResult Update([FromQuery] int id, [FromBody] TDto dto)
    {
        try
        {
            var item = _uow.GetObjectByKey<TModel>(id);
            if (item == null) return NotFound();
            item.Name = dto.Name;
            item.Active = dto.Active;
            item.DateModified = DateTime.UtcNow;
            _uow.CommitChanges();
            return Ok(ToDto(item));
        }
        catch (Exception ex) { return StatusCode(500, ex.Message); }
    }

    [HttpDelete("Delete[controller]ById")]
    public IActionResult Delete([FromQuery] int id)
    {
        try
        {
            var item = _uow.GetObjectByKey<TModel>(id);
            if (item == null) return NotFound();
            _uow.Delete(item);
            _uow.CommitChanges();
            return Ok($"Deleted {id}");
        }
        catch (Exception ex) { return StatusCode(500, ex.Message); }
    }
}
