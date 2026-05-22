using DevExpress.Xpo;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Tsp;

[ApiController]
[Route("[controller]")]
[ApiKey]
public class TiketMessageController: ControllerBase
{
    private readonly UnitOfWork _uow;
    public TiketMessageController(UnitOfWork uow)
    {
        _uow = uow;
    }
    private User CurrentUser => (User)HttpContext.Items["CurrentUser"]!;

    // Проверка права
    private bool HasPermission(string code) =>
        CurrentUser?.RoleUser?.RolePermissions
            .Any(rp => rp.Permission.Name == code
                    && rp.Permission.IsActive) ?? false;

    [HttpGet("GetAllMessages")]
    public IActionResult GetAll()
    {
        if (!HasPermission(PermisionConstant.TicketRead))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });
        try
        {
            var files = _uow.Query<TiketMessage>().ToList();
            Console.WriteLine($"Найдено тикетов: {files.Count}");

            var resault = files.Select(m => new TiketMessageResponseDto
            {
                Id = m.Oid,
                Tiket = m.Tiket.Oid,
                IsUser = m.IsUser,
                AuthorOid = m.Author != null ? m.Author.Oid : 0,
                AuthorName = m.Author != null ? m.Author.Name : "",
                MailMessageId = m.EmailMessageId,
                CreatedAt = m.CreatedAt,
                MessageText = m.MessageText ?? string.Empty,
            }).ToList();

            return Ok(resault);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка: {ex.Message}");
        }

    }

    [HttpGet("GetByTicketId")]
    public IActionResult GetMessages([FromQuery]int? tiketOid)
    {
        if (!HasPermission(PermisionConstant.TicketRead))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });
        try
        {
            IQueryable<TiketMessage> query = _uow.Query<TiketMessage>();

            if (tiketOid.HasValue)
                query = query.Where(f => f.Tiket != null && f.Tiket.Oid == tiketOid.Value);

            var resault = query.Select(m => new TiketMessageResponseDto
            {
                Id = m.Oid,
                Tiket = m.Tiket.Oid,
                IsUser = m.IsUser,
                AuthorOid = m.Author != null ? m.Author.Oid : 0,
                AuthorName = m.Author != null ? m.Author.Name : "",
                MailMessageId = m.EmailMessageId,
                CreatedAt = m.CreatedAt,
                MessageText = m.MessageText ?? string.Empty,
            }).ToList();

            return Ok(resault);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка: {ex.Message}");
        }

    }
    [HttpPost("CreateMessage")]
    public IActionResult Create([FromBody] TiketMessageDto model)
    {
        if (!HasPermission(PermisionConstant.TicketCreate))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });

        if (model == null)
            return BadRequest("Data is null");

        var tiket = _uow.Query<Tiket>().FirstOrDefault(u => u.Oid == model.Tiket);
        var autor = _uow.Query<User>().FirstOrDefault(u => u.Oid == model.Author);
       


        var message = new TiketMessage(_uow)
        {
            Oid = model.Id,
            Tiket = tiket,
            User = autor,
            IsRead = true,
            IsUser = true,
            MessageText = model.MessageText,
            CreatedAt= DateTime.Now,
        };

        _uow.CommitChanges();
        var emailService = new EmailQueueService(_uow);

        emailService.EnqueueTicketAnswer(
            tiket,
            autor,
            
            message.MessageText,
            model.Email
            );


        return Ok(new TiketMessageResponseDto
        {
            Tiket = message.Tiket.Oid,
            MessageText = message.MessageText,
            CreatedAt= message.CreatedAt,
        });
    }
}