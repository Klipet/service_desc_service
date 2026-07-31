
using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Internal;
using System.Linq.Expressions;
using System.Threading.Tasks;
using static DevExpress.Data.Helpers.ExpressiveSortInfo;

[ApiController]
[Route("[controller]")]
[ApiKey]
public class TiketController : ControllerBase
{
    private readonly IHubContext<TicketHub> _hub;
    private readonly UnitOfWork _uow;
    private readonly ILogger<TiketController> _logger;
    public TiketController(UnitOfWork uow, IHubContext<TicketHub> hub, ILogger<TiketController> logger)
    {
        _uow = uow;
        _hub = hub;
        _logger = logger;
    }

    private User CurrentUser => (User)HttpContext.Items["CurrentUser"]!;

    // Проверка права
    private bool HasPermission(string code) =>
        CurrentUser?.RoleUser?.RolePermissions
            .Any(rp => rp.Permission.Name == code
                    && rp.Permission.IsActive) ?? false;

    [HttpGet("GetAllTicket")]
    public IActionResult GetAll()
    {
        if (!HasPermission(PermisionConstant.TicketRead))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });
        try
        {
           
            var result = _uow.Query<Tiket>()
                .OrderBy(t => t.Oid)
                .AsEnumerable()       // ✅ переходим в память для ToDto
                .Select(ToDto)
                .ToList();
         
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка: {ex.Message}");
        }
    }

    [HttpPost("NewTicket")]
    public async Task<IActionResult> Create([FromBody] TiketPostDto model)
    {
        if (!HasPermission(PermisionConstant.TicketCreate))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });
        try{
            bool isEditing = model?.Id.HasValue == true && model?.Id.Value != 0;
            if (model == null) return BadRequest("Data is null");


            User user;
            if (model.UserId == 0)
            {
                user = SettingTiket.GetUserRoundRobin(_uow, workSpaceId: model.WorkSpaceId, logger: _logger);
                _logger.LogInformation("user = {User}", user == null ? "NULL" : user.Oid.ToString());
            }
            else
            {
                user = GetOrFail<User>(_uow, model.UserId, "User");
            }

            //   var users = GetOrFail<User>(uow, model.UserId, "User");
            var wp = GetOrFail<WorkSpace>(_uow, model.WorkSpaceId, "WorkSpace");
            var st = GetOrFail<State>(_uow, model.StateId, "State");
            var tt = GetOrFail<TiketType>(_uow, model.TypeTiketId, "TiketType");
            var pr = GetOrFail<Preority>(_uow, model.PreorityId, "Preority");
            var md = GetOrFail<Mode>(_uow, model.ModeId, "Mode");
            var sc = GetOrFail<SubCategory>(_uow, model.SubCategoryId, "SubCategory");
            var cat = GetOrFail<Category>(_uow, model.CategoryId, "Category");
            var au = GetOrFail<Author>(_uow, model.AuthorId, "Author");
            var pl = GetOrFail<Platform>(_uow, model.PlatformId, "Platform");
            var com = GetOrFail<Company>(_uow, model.CompanyId, "Company");



            Tiket ticket;
            if (isEditing)
            {
                ticket = GetOrFail<Tiket>(_uow, model.Id.Value, "Tiket");
            }
            else
            {
                ticket = new Tiket(_uow)
                {
                    DataCreted = DateTime.UtcNow
                };
            }

            ticket.Title = model.Title;
            ticket.Description = model.Description;
            ticket.Author = au;
            ticket.Category = cat;
            ticket.Company = com;
            ticket.SubCategory = sc;
            ticket.State = st;
            ticket.TypeTiket = tt;
            ticket.Platform = pl;
            ticket.WorkSpace = wp;
            ticket.User = user;
            ticket.Preorety = pr;
            ticket.BugNumber = model.BugNumber;
            ticket.BugTransfer = model.BugTransfer;
            ticket.Mode = md;
            ticket.IsActive = model.IsActive;

            try
            {
                _uow.CommitChanges();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error saving to database: {ex.Message}");
            }
            var resault = ToDto(ticket);
            await _hub.Clients.All.SendAsync(isEditing ? "TicketUpdated" : "NewTicketCreated", resault);
            return Ok(resault);
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Ошибка при создании тикета. Payload: {@Model}. StackTrace: {StackTrace}", model, ex.StackTrace);
            throw;
        }
    }

    [HttpGet("GetTicketById")]
    public IActionResult GetById([FromQuery] int id)
    {
        if (!HasPermission(PermisionConstant.TicketRead))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });
        var t = _uow.GetObjectByKey<Tiket>(id);
        if (t == null) return NotFound();
        return Ok(ToDto(t));
    }

    [HttpGet("GetTicketByUserId")]
    public IActionResult GetByUserId([FromQuery] int? userId = null)
    {
        if (!HasPermission(PermisionConstant.TicketRead))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });
        var query = _uow.Query<Tiket>();

        if (!userId.HasValue)
            return BadRequest(new { error = "userId is required" });

        var tickets = query
            .Where(t => t.User.Oid == userId.Value)
            .OrderByDescending(t => t.DataCreted)
            .Select(ToDto)
            .ToList();

        return Ok(tickets);
    }

    [HttpPut("Update")]
    public IActionResult Update([FromQuery] int id, [FromBody] TiketPostDto dto)
    {
        if (!HasPermission(PermisionConstant.TicketUpdate))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });
        try
        {

            var tiket = _uow.Query<Tiket>().FirstOrDefault(t => t.Oid == id);
            Console.WriteLine($"Найдено тикетов: {tiket.Oid}");
            if (tiket == null) return NotFound($"Тикет с Id {id} не найден");

            var user = GetOrFail<User>(_uow, dto.UserId, "Пользователь");
            var wp = GetOrFail<WorkSpace>(_uow, dto.WorkSpaceId, "WorkSpace");
            var st = GetOrFail<State>(_uow, dto.StateId, "State");
            var tt = GetOrFail<TiketType>(_uow, dto.TypeTiketId, "TiketType");
            var pr = GetOrFail<Preority>(_uow, dto.PreorityId, "Preority");
            var md = GetOrFail<Mode>(_uow, dto.ModeId, "Mode");
            var sc = GetOrFail<SubCategory>(_uow, dto.SubCategoryId, "SubCategory");
            var cat = GetOrFail<Category>(_uow, dto.CategoryId, "Category");
            var au = GetOrFail<Author>(_uow, dto.AuthorId, "Autor");
            var pl = GetOrFail<Platform>(_uow, dto.PlatformId, "Platform");
            var com = GetOrFail<Company>(_uow, dto.CompanyId, "Company");

            tiket.Title = dto.Title;
            tiket.Description = dto.Description;
            tiket.Author = au;
            tiket.Category = cat;
            tiket.Company = com;
            tiket.SubCategory = sc;
            tiket.State = st;
            tiket.TypeTiket = tt;
            tiket.Platform = pl;
            tiket.WorkSpace = wp;
            tiket.User = user;
            tiket.Preorety = pr;
            tiket.BugNumber = dto.BugNumber;
            tiket.BugTransfer = dto.BugTransfer;
            tiket.Mode = md;
            tiket.DataCreted = dto.DataCreted;
            tiket.IsActive = dto.IsActive;


            _uow.CommitChanges();

            return Ok($"Tiket {id} update. Дата выполнения:{tiket.DueDate}");
        }
        catch (Exception ex)
        {
            return StatusCode(ex.HResult, $"Ошибка при обновлении: {ex.Message}");
        }
    }




    [HttpDelete("Delete")]
    public IActionResult Delete([FromQuery] int id)
    {
        if (!HasPermission(PermisionConstant.TicketDelete))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });
        try
        {
            // Ищем через сессию явно
            var ticket = _uow.Query<Tiket>()
            .FirstOrDefault(t => t.Oid == id);


            if (ticket == null)
                return NotFound($"Тикет с id={id} не найден");

            ticket.IsActive = false;
            // Удаляем
            _uow.Delete(ticket);
            _uow.CommitChanges();

            return NoContent(); // 204
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка при удалении: {ex.Message}");
        }
    }

    [HttpGet("GetByPaging")]
    public IActionResult GetPaging(
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromQuery] int? state = null,
        [FromQuery] int? user = null,
        [FromQuery] int? company = null
        )
    {
        if (!HasPermission(PermisionConstant.TicketRead))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });

        try
        {
            IQueryable<Tiket> query = _uow.Query<Tiket>();

            // Фильтрация
            if (state.HasValue)
                query = query.Where(t => t.State != null && t.State.Oid == state.Value);

            if (user.HasValue)
                query = query.Where(t => t.User != null && t.User.Oid == user.Value);

            if (company.HasValue)
                query = query.Where(t => t.Company != null && t.Company.Oid == company.Value);

            query = query.OrderBy(t => t.Oid);

            var totalCount = query.Count();
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;


            var result = query
           .OrderBy(t => t.Oid)
           .Skip((page - 1) * pageSize)
           .Take(pageSize)
           .AsEnumerable()       // ✅ переходим в память для ToDto
           .Select(ToDto)
           .ToList();

            return Ok(new
            {
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Page = page,
                PageSize = pageSize,
                tikets = result
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка: {ex.Message}");
        }
    }


    [HttpGet("GetSearch")]
    public IActionResult GetSearch(
        [FromQuery] int? page = null,
        [FromQuery] int? pageSize = null,
        [FromQuery] string search = null)
    {
        IQueryable<Tiket> query = new XPQuery<Tiket>(_uow);

        // 🔍 поиск (без убийства сервера)
        if (!string.IsNullOrWhiteSpace(search))
        {
            if (int.TryParse(search, out int id))
            {
                query = query.Where(t => t.Oid == id);
            }
            else
            {
                query = query.Where(t =>
                    t.Title.Contains(search) ||
                    t.Company.Name.Contains(search) ||
                    t.User.Name.Contains(search) ||
                    t.Author.Name.Contains(search)
                    );
            }
        }
        else
        {
            return Ok(Array.Empty<TiketResponseDto>());
        }
        // 📦 если пагинации НЕТ
        if (page == null && pageSize == null)
        {
            var data = query
                .OrderByDescending(t => t.DataCreted)
                .Take(100) // 💥 защита сервера
                .Select(ToDto)
                .ToList();

            return Ok(data);
        }

        // 📄 если пагинация есть
        int currentPage = page ?? 1;
        int size = pageSize ?? 20;

        var totalCount = query.Count();

        var pagedData = query
            .OrderByDescending(t => t.DataCreted)
            .Skip((currentPage - 1) * size)
            .Take(size)
            .Select(ToDto)
            .ToList();

        return Ok(new
        {
            total = totalCount,
            page = currentPage,
            pageSize = size,
            data = pagedData
        });

    }

    [HttpPost("MargeTickets")]
    public IActionResult ServiceMargeTickets([FromBody] MenageTiketRequest request)
    {
        if (!HasPermission(PermisionConstant.TicketUpdate))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });

        Tiket primaryTiket = _uow.GetObjectByKey<Tiket>(request.PrymoryTiketId);

        try
        {
            if (primaryTiket == null)
                return NotFound(new { error = $"Заявка #{request.PrymoryTiketId} не найдена", errorCode = 404 });

            if (request.SecindTiketId == null || request.SecindTiketId.Count == 0)
                return BadRequest(new { error = "Не указаны заявки для объединения", errorCode = 400 });

            if (request.SecindTiketId.Contains(request.PrymoryTiketId))
                return BadRequest(new { error = "Нельзя объединить заявку с самой собой", errorCode = 400 });

            foreach (int secondId in request.SecindTiketId)
            {
                Tiket secondTiket = _uow.GetObjectByKey<Tiket>(secondId);

                if (secondTiket == null)
                    return NotFound(new { error = $"Заявка #{secondId} не найдена", errorCode = 404 });

                if (secondTiket.IsActive == false)
                    return BadRequest(new { error = $"Заявка #{secondId} не активна или удалена", errorCode = 400 });

                // 2. Переносим все сообщения второй заявки в первую
                XPCollection<TiketMessage> messages = new XPCollection<TiketMessage>(
                    _uow,
                    CriteriaOperator.Parse("Tiket = ?", secondId)
                );
                foreach (TiketMessage message in messages)
                {
                    message.Tiket = primaryTiket;
                }

                TiketMessage ticketMessage = new TiketMessage(_uow)
                {
                    Tiket = primaryTiket,
                    MessageText = secondTiket.Description,
                    Author = secondTiket.Author,
                    EmailMessageId = secondTiket.MessageId,
                    CreatedAt = secondTiket.DataCreted,
                    IsRead = false,
                    IsUser = false
                };

                // 3. Переносим все файлы второй заявки в первую
                XPCollection<TiketFile> files = new XPCollection<TiketFile>(_uow);
                files.Criteria = CriteriaOperator.Parse("Tiket = ?", secondId);
                foreach (TiketFile file in files)
                {
                    file.Tiket = primaryTiket;
                }

                // 4. Удаляем вторую заявку
                _uow.Delete(secondTiket);
            }

            // 5. Сохраняем всё в транзакции
            _uow.CommitChanges();
            return Ok(new
            {
                state = 200,
                message = $"Заявка #{request.PrymoryTiketId} успешно объединена с заявками: {string.Join(", ", request.SecindTiketId)}"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message, errorCode = 500 });
        }
    }



    private T GetOrFail<T>(UnitOfWork uow, int id, string entityName) where T : XPObject
    {
        _logger.LogInformation("GetOrFail: ищем {EntityName} с Oid={Id}", entityName, id);

        var entity = uow.Query<T>().FirstOrDefault(e => e.Oid == id);
        if (entity == null)
        {
            _logger.LogWarning("GetOrFail: {EntityName} с Oid={Id} не найден", entityName, id);
            throw new KeyNotFoundException($"{entityName} не найден");
        }
        return entity;
    }


    private TiketResponseDto ToDto(Tiket t) => new TiketResponseDto
    {
       
        Id = t.Oid,
        Title = t.Title ?? string.Empty,
        Description = t.Description ?? string.Empty,
        AuthorId = t.Author != null ? t.Author.Oid : 0,
        AuthorName = t.Author != null ? t.Author.Name : string.Empty,
        CategoryId = t.Category != null ? t.Category.Oid : 0,
        CategoryName = t.Category != null ? t.Category.Name : string.Empty,
        CompanyId = t.Company != null ? t.Company.Oid : 0,
        CompanyName = t.Company != null ? t.Company.Name : string.Empty,
        SubCategoryId = t.SubCategory != null ? t.SubCategory.Oid : 0,
        SubCategoryName = t.SubCategory != null ? t.SubCategory.Name : string.Empty,
        StateName = t.State != null ? t.State.Name : string.Empty,
        StateId = t.State != null ? t.State.Oid : 0,
        TypeTiketName = t.TypeTiket != null ? t.TypeTiket.Name : string.Empty,
        TypeTiketId = t.TypeTiket != null ? t.TypeTiket.Oid : 0,
        PlatformId = t.Platform != null ? t.Platform.Oid : 0,
        PlatformName = t.Platform != null ? t.Platform.Name : string.Empty,
        WorkSpaceId = t.WorkSpace != null ? t.WorkSpace.Oid : 0,
        WorkSpaceName = t.WorkSpace != null ? t.WorkSpace.Name : string.Empty,
        UserId = t.User != null ? t.User.Oid : 0,
        UserName = t.User != null ? t.User.Name : string.Empty,
        PreorityId = t.Preorety != null ? t.Preorety.Oid : 0,
        PreorityName = t.Preorety != null ? t.Preorety.Name : string.Empty,
        BugNumber = t.BugNumber ?? string.Empty,
        BugTransfer = t.BugTransfer,
        ModeId = t.Mode != null ? t.Mode.Oid : 0,
        ModeName = t.Mode != null ? t.Mode.Name : string.Empty,
        DataCreted = t.DataCreted,
        DueDate = t.DueDate ?? DateTime.UtcNow,

        Phones = t.Phones.Select(p => new TicketPhoneResponseDto
        {
            Id = p.Oid,
            TicketId = p.Ticket.Oid != null ? p.Ticket.Oid : 0,
            UserId = p.User != null ? p.User.Oid : 0,
            UserName = p.User != null ? p.User.Name : string.Empty,
            ResultPhoneName = p.PhoneResults != null ? p.PhoneResults.Name : string.Empty,
            ResultPhoneOid = p.PhoneResults != null ? p.PhoneResults.Oid : 0,
            Phone = p.Phone ?? string.Empty,
            DataCreated = p.DataCreated,
            IsActive = p.IsActive,
            Comment = p.Comment ?? string.Empty
        }).ToList(),

        Files = t.Files.Select(f => new TiketFileDto
        {
            Id = f.Oid,
            FileName = f.FileName,
            FileUrl = f.FileUrl,
            IsResponse = f.IsResponse
        }).ToList(),

        Solution = t.Solutions.Select(s => new TiketSolutionDto
        {
            Id = s.Oid,
            Author = s.Author != null ? s.Author.Oid : 0,
            User = s.User != null ? s.User.Oid : 0,
            MessageText = s.MessageText ?? string.Empty,
            CreatedAt = s.CreatedAt,
            EmailList = string.IsNullOrEmpty(s.EmailList)
                ? new List<string> { s.Author.Email }
                : s.EmailListParsed
        }).ToList(),

        Message = t.Messages.Select(m => new TiketMessageResponseDto
        {
            Id = m.Oid,
            Tiket = m.Tiket.Oid,
            IsUser = m.IsUser,
            AuthorOid = m.Author != null ? m.Author.Oid : 0,
            AuthorName = m.Author != null ? m.Author.Name : "",
            MailMessageId = m.EmailMessageId,
            CreatedAt = m.CreatedAt,
            MessageText = m.MessageText ?? string.Empty,
        }).ToList(),
    };

}
