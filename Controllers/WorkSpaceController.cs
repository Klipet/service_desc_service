
using DevExpress.Xpo;
using Microsoft.AspNetCore.Mvc;
[ApiController]
[Route("api/[controller]")]
public class WorkSpaceController : ControllerBase
{
    private readonly UnitOfWork _uow;
    public WorkSpaceController(UnitOfWork uow)
    {
        _uow = uow;
    }
    [HttpGet]
    public IActionResult GetAllWorkSpace()
    {
        try
        {
            var workSp = _uow.Query<WorkSpace>().ToList();
            var resaultWp = workSp.Select(work => new WorkSpaceDto
            {
                Oid = work.Oid,
                Name = work.Name,
                Active = work.Active,
                DateCreated = work.DateCreated,
            }).ToList();
            return Ok(resaultWp);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка: {ex.Message}");
        }
    }


    [HttpPost]
    public IActionResult CreateUser([FromBody] WorkSpaceDto wp)
    {
        if (wp == null)
            return BadRequest("Model is null");
        var workSp = new WorkSpace(_uow)
        {
            Oid = wp.Oid,
            Name = wp.Name,
            Active = wp.Active,
            DateCreated = DateTime.UtcNow,
        };
        try
        {
            _uow.CommitChanges();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error saving to database: {ex.Message}");
        }
        var resaultWp = new WorkSpaceDto
        {
            Oid = workSp.Oid,
            Name = workSp.Name,
            Active = workSp.Active,
            DateCreated = DateTime.UtcNow,
        };

        return Ok(workSp);
    }

}

