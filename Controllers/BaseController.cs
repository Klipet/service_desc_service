using DevExpress.Xpo;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
[ApiKey]
public abstract class BaseController<TModel, TDto> : ControllerBase
    where TModel : BaseEntity
    where TDto : BaseDto, new()
{
    protected readonly UnitOfWork _uow;

    protected BaseController(UnitOfWork uow)
    {
        _uow = uow;
    }

    protected User CurrentUser => (User)HttpContext.Items["CurrentUser"]!;
    protected bool HasPermission(string code) =>
        CurrentUser?.RoleUser?.RolePermissions
            .Any(rp => rp.Permission.Name == code
                    && rp.Permission.IsActive) ?? false;

    protected bool HasAllPermissions(params string[] codes) =>
        codes.All(code => HasPermission(code));

    protected IActionResult Forbidden() =>
        StatusCode(403, new { error = "Access denied", userRole = CurrentUser?.RoleUser?.Name });

    protected abstract string PermissionRead { get; }
    protected abstract string PermissionCreate { get; }
    protected abstract string PermissionUpdate { get; }
    protected abstract string PermissionDelete { get; }


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
    public  IActionResult GetAll()
    {
        if(!HasPermission(PermissionRead)) return Forbidden();
        try
        {
            var items = _uow.Query<TModel>().ToList();
            return Ok(items.Select(ToDto));
        }
        catch (Exception ex) { return StatusCode(500, ex.Message); }
    }

    [HttpGet("Get[controller]ById")]
    public  IActionResult GetById([FromQuery] int id)
    {
        if (!HasPermission(PermissionRead)) return Forbidden();
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
        if (!HasPermission(PermissionCreate)) return Forbidden();
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
        if (!HasPermission(PermissionUpdate)) return Forbidden();
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
    public virtual IActionResult Delete([FromQuery] int id)
    {
        if (!HasPermission(PermissionDelete)) return Forbidden();
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
