using DevExpress.Xpo;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
[ApiKey]
public class TicketPhoneController: ControllerBase
{
    private readonly UnitOfWork _uow;
    private readonly ILogger<TiketController> _logger;
    public TicketPhoneController(UnitOfWork uow, ILogger<TiketController> logger)
    {
        _uow = uow;
        _logger = logger;
    }
    private User CurrentUser => (User)HttpContext.Items["CurrentUser"]!;

    private bool HasPermission(string code) =>
    CurrentUser?.RoleUser?.RolePermissions
        .Any(rp => rp.Permission.Name == code
                && rp.Permission.IsActive) ?? false;

    [HttpGet("GetAll[controller]")]
    public IActionResult GetAll()
    {
        if (!HasPermission(PermisionConstant.TicketRead))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });
        try
        {
            var ticketPhones = _uow.Query<TicketPhone>();
            var result = ticketPhones
                .ToList() // материализуем, дальше маппим в памяти
                .Select(MapToResponseDto)
                .ToList();


            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка: {ex.Message}");
        }
    }

    [HttpGet("GetByTicket[controller]")]
    public IActionResult GetByTicket([FromQuery] int ticketId)
    {
        if (!HasPermission(PermisionConstant.TicketRead))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });
        try
        {
            var phoneResault = _uow.Query<TicketPhone>().Where(tp => tp.Ticket.Oid == ticketId);
            var result = phoneResault
                .ToList() // материализуем, дальше маппим в памяти
                .Select(MapToResponseDto)
                .ToList();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка: {ex.Message}");
        }
    }

    [HttpPost("AddCall")]
    public IActionResult Create([FromBody] TicketPhonePostDto phoneModel)
    {
        if (!HasPermission(PermisionConstant.TiketTypeUpdate))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });
        try
        {
            User user;
            Tiket tiket;
            PhoneResult resault;
            if (phoneModel == null)
                return BadRequest("Data is null");
            if(phoneModel.UserId != null)
            {
                user = _uow.Query<User>().FirstOrDefault(u => u.Oid == phoneModel.UserId);
            }
            else
            {
                return BadRequest("User not faund");
            }
            
            if (phoneModel.TicketId != null)
            {
                tiket = _uow.Query<Tiket>().FirstOrDefault(t => t.Oid == phoneModel.TicketId);
            }
            else
            {
                return BadRequest("Ticket not faund");
            }
           
            if (phoneModel.ResultPhoneOid != null)
            {
                resault = _uow.Query<PhoneResult>().FirstOrDefault(r => r.Oid == phoneModel.ResultPhoneOid);
            }
            else
            {
                return BadRequest("Phone Result not faund");
            }

            var resaultPhoneDb = new TicketPhone(_uow)
            {
                Ticket = tiket,
                User = user,
                Phone = phoneModel.Phone,
                PhoneResults = resault,
                DataCreated = DateTime.Now,
                DataModified = DateTime.Now,
                IsActive = phoneModel.IsActive ?? true,
                Comment = phoneModel.Comment,
            };
            _uow.CommitChanges();
            return Ok(new
            {
               Oid = resaultPhoneDb.Oid,
            });

        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка: {ex.Message}");
        }
    }

    [HttpPut("Updade[controller]")]
    public IActionResult UpdateCallPhone([FromBody] TicketPhonePostDto model, [FromQuery] int oidUpdat)
    {
        if (!HasPermission(PermisionConstant.TiketTypeUpdate))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });

        try
        {
            var phoneResault = _uow.Query<TicketPhone>().FirstOrDefault(tp => tp.Oid == oidUpdat);
            if (phoneResault == null) return NotFound("Ticket Phone не найден");

            if (model.TicketId.HasValue)
            {
                var tiket = _uow.Query<Tiket>().FirstOrDefault(t => t.Oid == model.TicketId.Value);
                if (tiket == null) return NotFound("Tiket не найден");
                phoneResault.Ticket = tiket;
            }

            if (model.UserId.HasValue)
            {
                var user = _uow.Query<User>().FirstOrDefault(u => u.Oid == model.UserId.Value);
                if (user == null) return NotFound("Пользователь не найден");
                phoneResault.User = user;
            }

            if (model.ResultPhoneOid.HasValue)
            {
                var resault = _uow.Query<PhoneResult>().FirstOrDefault(u => u.Oid == model.ResultPhoneOid.Value);
                if (resault == null) return NotFound("PhoneResult не найден");
                phoneResault.PhoneResults = resault;
            }

            if (model.Phone != null)
                phoneResault.Phone = model.Phone;

            if (model.Comment != null)
                phoneResault.Comment = model.Comment;

            if (model.IsActive.HasValue)
                phoneResault.IsActive = model.IsActive.Value;

            phoneResault.DataModified = DateTime.Now;

            _uow.CommitChanges();

            var responsePhonetiket = MapToResponseDto(phoneResault);

            return Ok(responsePhonetiket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении TicketPhone (oidUpdat={OidUpdat})", oidUpdat);
            return StatusCode(500, new { error = "Внутренняя ошибка сервера", errorCode = 500 });
        }
    }


    private static TicketPhoneResponseDto MapToResponseDto(TicketPhone entity)
    {
        return new TicketPhoneResponseDto
        {
            Id = entity.Oid,
            TicketId = entity.Ticket?.Oid ?? 0,
            UserId = entity.User?.Oid,
            UserName = entity.User?.Name,
            Phone = entity.Phone,
            ResultPhoneOid = entity.PhoneResults?.Oid ?? 0,
            ResultPhoneName = entity.PhoneResults?.Name,
            Comment = entity.Comment,
            DataCreated = entity.DataCreated,
            IsActive = entity.IsActive
        };
    }
}