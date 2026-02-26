using DevExpress.Xpo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using static DevExpress.Data.Helpers.ExpressiveSortInfo;


[ApiController]
[Route("api")]
public class TiketSolutionController: ControllerBase
{
    private readonly UnitOfWork _uow;
    public TiketSolutionController(UnitOfWork uow)
    {
        _uow = uow;
    }

    [HttpPost("Solution")]
    public IActionResult Create([FromBody] TiketSolutionDto model)
    {
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

        if (solution.EmailList != null && solution.EmailList.Any())
        {
            var emails = solution.EmailList
                .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(e => e.Trim())
                .Where(e => !string.IsNullOrWhiteSpace(e));

            foreach (var email in emails)
            {
                new EmailQueue(_uow)
                {
                    To = email,
                    Subject = $"Ответ по тикету #{tiket.Oid}",
                    Body = solution.MessageText,
                    IsSent = false,
                    CreatedAt = DateTime.UtcNow
                };
            }
            _uow.CommitChanges();
        }
        else
        {
            new EmailQueue(_uow)
            {
                To = autor.Email,
                Subject = $"Ответ по тикету #{tiket.Oid}",
                Body = solution.MessageText,
                IsSent = false,
                CreatedAt = DateTime.UtcNow
            };
            _uow.CommitChanges();
        }
            return Ok();
    }

    [HttpGet("email-status/{tiketId}")]
    public IActionResult GetEmailStatus(int tiketId)
    {
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
