using DevExpress.Xpo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using static DevExpress.Data.Helpers.ExpressiveSortInfo;

[ApiController]
[Route("[controller]")]
[ApiKey]
public class TiketSolutionController: ControllerBase
{
    private readonly UnitOfWork _uow;
    public TiketSolutionController(UnitOfWork uow)
    {
        _uow = uow;
    }

    private User CurrentUser => (User)HttpContext.Items["CurrentUser"]!;

    // Проверка права
    private bool HasPermission(string code) =>
        CurrentUser?.RoleUser?.RolePermissions
            .Any(rp => rp.Permission.Name == code
                    && rp.Permission.IsActive) ?? false;

    [HttpPost("Solution")]
    public IActionResult Create([FromBody] TiketSolutionDto model)
    {
        if (!HasPermission(PermisionConstant.TicketUpdate))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });

        if (model == null) return BadRequest("Data is null");

        var tiket = GetOrFail<Tiket>(_uow, model.Tiket, "Tiket");

        var autor = _uow.Query<Author>().FirstOrDefault(a => a.Oid == tiket.Author.Oid );
        var user = _uow.Query<User>().FirstOrDefault(a => a.Oid == model.User );

        var solution = new TiketSolution(_uow)
        {
            Tiket = tiket,
            Author = autor,
            User = user,
            MessageText = model.MessageText,
            EmailListParsed = model.EmailList
        };
        _uow.CommitChanges();

        // 🔥 вызываем сервис очереди
        var emailService = new EmailQueueService(_uow);

        emailService.EnqueueTicketAnswer(
            tiket,
            user,
            solution.MessageText,
            model.EmailList
            );

        var resault = new TiketSolutionDto
        {
            Id = solution.Oid,
            Tiket = solution.Tiket.Oid,
            Author = solution.Author.Oid,
            User = solution.User.Oid,
            CreatedAt = solution.CreatedAt,
            MessageText = solution.MessageText,
            EmailList = solution.EmailListParsed
        };
        return Ok(resault);
    }

    [HttpGet("EmailStatis[controller]")]
    public IActionResult GetEmailStatus([FromQuery] int tiketId)
    {
        if (!HasPermission(PermisionConstant.TicketRead))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });

        var emails = _uow.Query<EmailQueue>()
            .Where(e => e.Subject.Contains($"#{tiketId}"))
            .Select(e => new { e.To, e.IsSent, e.ErrorMessage, e.SentAt })
            .ToList();
        return Ok(emails);
    }

    private T GetOrFail<T>(UnitOfWork uow, int id, string entityName) where T : XPObject
    {
        var entity = uow.Query<T>().FirstOrDefault(e => e.Oid == id);
        if (entity == null)
            throw new KeyNotFoundException($"{entityName} не найден");
        return entity;
    }
}
