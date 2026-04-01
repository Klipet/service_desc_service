using DevExpress.Xpo;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
[ApiKey]
public class TiketFileController : ControllerBase
{
    private readonly UnitOfWork _uow;
    public TiketFileController(UnitOfWork uow)
    {
        _uow = uow;
    }
    private User CurrentUser => (User)HttpContext.Items["CurrentUser"]!;

    // Проверка права
    private bool HasPermission(string code) =>
        CurrentUser?.RoleUser?.RolePermissions
            .Any(rp => rp.Permission.Name == code
                    && rp.Permission.IsActive) ?? false;

    [HttpGet("GetAllFiles")]
    public IActionResult GetAll()
    {
        if (!HasPermission(PermisionConstant.TicketRead))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });
        try
        {
            var files = _uow.Query<TiketFile>().ToList();
            Console.WriteLine($"Найдено тикетов: {files.Count}");

            var resault = files.Select(f => new TiketFileDto
            {
                Id = f.Oid,
                FileName = f.FileName,
                FileUrl = f.FileUrl,
                IsResponse = f.IsResponse,
                TiketOid = f.Tiket?.Oid ?? 0,
                TiketResponseOid = f.TiketSolution?.Oid ?? 0,

            }).ToList();

            return Ok(resault);
        }
        catch (Exception ex) 
        {
            return StatusCode(500, $"Ошибка: {ex.Message}");
        }

    }
    [HttpPost("CreateFile")]
    public IActionResult Create([FromBody] TiketFileDto model)
    {
        if (!HasPermission(PermisionConstant.TicketCreate))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });

        if (model == null)
            return BadRequest("Data is null");

        Tiket tiket = null;
        TiketSolution ts = null;

        if (model.IsResponse)
        {
            ts = _uow.Query<TiketSolution>()
                     .FirstOrDefault(r => r.Oid == model.TiketResponseOid);

            if (ts == null)
                return NotFound("TiketSolution не найден");
        }
        else
        {
            tiket = _uow.Query<Tiket>()
                        .FirstOrDefault(r => r.Oid == model.TiketOid);

            if (tiket == null)
                return NotFound("Tiket не найден");
        }

        var file = new TiketFile(_uow)
        {
            Oid = model.Id,
            FileName = model.FileName,
            FileUrl = model.FileUrl,
            IsResponse = model.IsResponse,
            Tiket = tiket,
            TiketSolution = ts
        };

        _uow.CommitChanges();

        return Ok(new
        {
            file.Oid,
            file.FileName,
            file.FileUrl,
            file.IsResponse,
            file.Tiket,
            file.TiketSolution,
        });
    }
    [HttpGet("GetFileById")]
    public IActionResult GetFiles([FromQuery]int? tiketOid, [FromQuery] int? solutionOid)
    {
        if (!HasPermission(PermisionConstant.TicketRead))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });
        IQueryable<TiketFile> query = _uow.Query<TiketFile>();

        if (tiketOid.HasValue)
            query = query.Where(f => f.Tiket != null && f.Tiket.Oid == tiketOid.Value);

        if (solutionOid.HasValue)
            query = query.Where(f => f.TiketSolution != null && f.TiketSolution.Oid == solutionOid.Value);

        var result = query.Select(f => new
        {
            f.Oid,
            f.FileName,
            f.FileUrl,
            f.IsResponse,
            f.Tiket,
            f.TiketSolution,
        }).ToList();

        return Ok(result);
    }
}