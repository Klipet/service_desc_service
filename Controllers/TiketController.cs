
using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Internal;
using System.Threading.Tasks;
using static DevExpress.Data.Helpers.ExpressiveSortInfo;

[ApiController]
[Route("[controller]")]
[ApiKey]
public class TiketController : ControllerBase
{
    private readonly IHubContext<TicketHub> _hub;
    private readonly UnitOfWork _uow;
    public TiketController(UnitOfWork uow, IHubContext<TicketHub> hub   )
    {
        _uow = uow;
        _hub = hub;
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
                .Select(t => new TiketResponseDto
                {
                    Id = t.Oid,
                    Title = t.Title ?? string.Empty,
                    Description = t.Description ?? string.Empty,

                    AuthorId = t.Author != null ? t.Author.Oid : 0,
                    AuthorName = t.Author != null ? t.Author.Name : string.Empty,

                    CategoryName = t.Category != null ? t.Category.Name : string.Empty,

                    Phone = t.Phone ?? string.Empty,

                    CompanyId = t.Company != null ? t.Company.Oid : 0,
                    CompanyName = t.Company != null ? t.Company.Name : string.Empty,

                    SubCategoryName = t.SubCategory != null ? t.SubCategory.Name : string.Empty,

                    StateName = t.State != null ? t.State.Name : string.Empty,
                    StateId = t.State != null ? t.State.Oid : 0,

                    TypeTiketName = t.TypeTiket != null ? t.TypeTiket.Name : string.Empty,

                    PlatformId = t.Platform != null ? t.Platform.Oid : 0,
                    PlatformName = t.Platform != null ? t.Platform.Name : string.Empty,

                    WorkSpaceName = t.WorkSpace != null ? t.WorkSpace.Name : string.Empty,

                    UserId = t.User != null ? t.User.Oid : 0,
                    UserName = t.User != null ? t.User.Name : string.Empty,

                    PreorityName = t.Preorety != null ? t.Preorety.Name : string.Empty,

                    DataPhone = t.DataPhone,
                    ResaultPhone = t.ResaultPhone,
                    DateSecondPhone = t.DateSecondPhone,
                    BugNumber = t.BugNumber ?? string.Empty,
                    BugTransfer = t.BugTransfer,
                    ModeName = t.Mode != null ? t.Mode.Name : string.Empty,
                    DataCreted = t.DataCreted,
                    DataModefire = t.DataModefire,
                    DueDate = t.DueDate ?? DateTime.UtcNow,

                    // 📎 Файлы тикета
                    Files = t.Files.Select(f => new TiketFileDto
                    {
                        Id = f.Oid,
                        FileName = f.FileName,
                        FileUrl = f.FileUrl,
                        IsResponse = f.IsResponse
                    }).ToList(),

                    // 💬 Решения
                    Solution = t.Solutions.Select(s => new TiketSolutionDto
                    {
                        Id = s.Oid,
                        Author = s.Author != null ? s.Author.Oid : 0,
                        User = s.User != null ? s.User.Oid : 0,
                        MessageText = s.MessageText ?? string.Empty,
                        CreatedAt = s.CreatedAt,

                        // если EmailList хранится строкой через ;
                        EmailList = string.IsNullOrEmpty(s.EmailList)
                            ? new List<string> { s.Author.Email }
                            : s.EmailListParsed
                    }).ToList()
                }).ToList();

            Console.WriteLine($"Найдено тикетов: {result.Count}");

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

        using var uow = MyXPO.GetNewUnitOfWork();
        if (model == null) return BadRequest("Data is null");
        User user;
        if (model.UserId == 0)
        {
           user = SettingTiket.GetUserRoundRobin(uow, workSpaceId: model.WorkSpaceId);

            Console.WriteLine($"userId = {user.Oid}");
        }
        else
        {
            user = GetOrFail<User>(uow, model.UserId, "User");
        }

     //   var users = GetOrFail<User>(uow, model.UserId, "User");
        var wp = GetOrFail<WorkSpace>(uow, model.WorkSpaceId, "WorkSpace");
        var st = GetOrFail<State>(uow, model.StateId, "State");
        var tt = GetOrFail<TiketType>(uow, model.TypeTiketId, "TiketType");
        var pr = GetOrFail<Preority>(uow, model.PreorityId, "Preority");
        var md = GetOrFail<Mode>(uow, model.ModeId, "Mode");
        var sc = GetOrFail<SubCategory>(uow, model.SubCategoryId, "SubCategory");
        var cat = GetOrFail<Category>(uow, model.CategoryId, "Category");
        var au = GetOrFail<Author>(uow, model.AuthorId, "Author");
        var pl = GetOrFail<Platform>(uow, model.PlatformId, "Platform");
        var com = GetOrFail<Company>(uow, model.CompanyId, "Company");



        var ticket = new Tiket(uow)
        {
            Title = model.Title,
            Description = model.Description,
            Author = au,
            Category = cat,
            Phone = model.Phone,
            Company = com,
            SubCategory = sc,
            State = st,
            TypeTiket = tt,
            Platform = pl,
            WorkSpace = wp,
            User = user,
            Preorety = pr,
            DataPhone = model.DataPhone,
            ResaultPhone = model.ResaultPhone,
            DateSecondPhone = model.DateSecondPhone,
            BugNumber = model.BugNumber,
            BugTransfer = model.BugTransfer,
            Mode = md,
            DataCreted = DateTime.UtcNow
        };
        try
        {
            uow.CommitChanges();
            

        }
        catch (Exception ex) {
            return StatusCode(500, $"Error saving to database: {ex.Message}");
        }
        var resault = new TiketResponseDto
        {
            Id = ticket.Oid,
            Title = ticket.Title,
            Description = ticket.Description,
            AuthorName = au.Name,
            AuthorId = au.Oid,
            CategoryName = cat.Name,
            Phone = ticket.Phone,
            CompanyName = com.Name,
            CompanyId = com.Oid,
            SubCategoryName = sc.Name,
            StateName = st.Name,
            TypeTiketName = tt.Name,
            PlatformName = pl.Name,
            PlatformId = pl.Oid,
            WorkSpaceName = ticket.WorkSpace.Name,
            UserName = user.Name,
            UserId = user.Oid,
            PreorityName = pr.Name,
            DataPhone = ticket.DataPhone,
            ResaultPhone = ticket.ResaultPhone,
            DateSecondPhone = ticket.DateSecondPhone,
            BugNumber = ticket.BugNumber,
            BugTransfer = ticket.BugTransfer,
            ModeId = md.Oid,
            ModeName = md.Name,
            DataCreted = ticket.DataCreted,
            DueDate = ticket.DueDate ?? DateTime.UtcNow,
        };
        await _hub.Clients.All.SendAsync("NewTicketCreated", resault);

        //   return CreatedAtAction("GetTiketById", new { id = ticket.Id }, resault);
        return Ok(resault);
    }

    [HttpGet("GetTicketById")]
    public IActionResult GetById([FromQuery] int id)
    {
        if (!HasPermission(PermisionConstant.TicketRead))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });

        var t = _uow.GetObjectByKey<Tiket>(id);
        if(t == null) return NotFound();

/*
        var user = _uow.GetObjectByKey<User>(tiket.User.Oid) ?? throw new KeyNotFoundException("Пользователь не найден");
        var wp = _uow.GetObjectByKey<WorkSpace>(tiket.WorkSpace.Oid) ?? throw new KeyNotFoundException("WorkSpace не найден");
        var st = _uow.GetObjectByKey<State>(tiket.State.Oid) ?? throw new KeyNotFoundException("State не найден");
        var tt = _uow.GetObjectByKey<TiketType>(tiket.TypeTiket.Oid) ?? throw new KeyNotFoundException("TiketType не найден");
        var pr = _uow.GetObjectByKey<Preority>(tiket.Preorety.Oid) ?? throw new KeyNotFoundException("Preority не найден");
        var md = _uow.GetObjectByKey<Mode>(tiket.Mode.Oid) ?? throw new KeyNotFoundException("Mode не найден");
        var sc = _uow.GetObjectByKey<SubCategory>(tiket.SubCategory.Oid) ?? throw new KeyNotFoundException("SubCategory не найден");
        var cat = _uow.GetObjectByKey<Category>(tiket.Category.Oid) ?? throw new KeyNotFoundException("Category не найден");
        var au = _uow.GetObjectByKey<Author>(tiket.Author.Oid) ?? throw new KeyNotFoundException("Author не найден");
        var pl = _uow.GetObjectByKey<Platform>(tiket.Platform.Oid) ?? throw new KeyNotFoundException("Platform не найден");
        var com = _uow.GetObjectByKey<Company>(tiket.Company.Oid) ?? throw new KeyNotFoundException("Company не найден");
*/
        var tiketDto = new TiketResponseDto
        {
            Id = t.Oid,
            Title = t.Title ?? string.Empty,
            Description = t.Description ?? string.Empty,

            AuthorId = t.Author != null ? t.Author.Oid : 0,
            AuthorName = t.Author != null ? t.Author.Name : string.Empty,

            CategoryName = t.Category != null ? t.Category.Name : string.Empty,

            Phone = t.Phone ?? string.Empty,

            CompanyId = t.Company != null ? t.Company.Oid : 0,
            CompanyName = t.Company != null ? t.Company.Name : string.Empty,

            SubCategoryName = t.SubCategory != null ? t.SubCategory.Name : string.Empty,

            StateName = t.State != null ? t.State.Name : string.Empty,
            StateId = t.State != null ? t.State.Oid : 0,

            TypeTiketName = t.TypeTiket != null ? t.TypeTiket.Name : string.Empty,

            PlatformId = t.Platform != null ? t.Platform.Oid : 0,
            PlatformName = t.Platform != null ? t.Platform.Name : string.Empty,

            WorkSpaceName = t.WorkSpace != null ? t.WorkSpace.Name : string.Empty,

            UserId = t.User != null ? t.User.Oid : 0,
            UserName = t.User != null ? t.User.Name : string.Empty,

            PreorityName = t.Preorety != null ? t.Preorety.Name : string.Empty,

            DataPhone = t.DataPhone,
            ResaultPhone = t.ResaultPhone,
            DateSecondPhone = t.DateSecondPhone,
            BugNumber = t.BugNumber ?? string.Empty,
            BugTransfer = t.BugTransfer,
            ModeName = t.Mode != null ? t.Mode.Name : string.Empty,
            DataCreted = t.DataCreted,
            DataModefire = t.DataModefire,
            DueDate = t.DueDate ?? DateTime.UtcNow,
            // 📎 Файлы тикета
            Files = t.Files.Select(f => new TiketFileDto
            {
                Id = f.Oid,
                FileName = f.FileName,
                FileUrl = f.FileUrl,
                IsResponse = f.IsResponse
            }).ToList(),

            // 💬 Решения
            Solution = t.Solutions.Select(s => new TiketSolutionDto
            {
                Id = s.Oid,
                Author = s.Author != null ? s.Author.Oid : 0,
                User = s.User != null ? s.User.Oid : 0,
                MessageText = s.MessageText ?? string.Empty,
                CreatedAt = s.CreatedAt,

                // если EmailList хранится строкой через ;
                EmailList = string.IsNullOrEmpty(s.EmailList)
                    ? new List<string> { s.Author.Email }
                    : s.EmailListParsed
            }).ToList()

        };
                 
        return Ok(tiketDto);

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
            tiket.Phone = dto.Phone;
            tiket.Company = com;
            tiket.SubCategory = sc;
            tiket.State = st;
            tiket.TypeTiket = tt;
            tiket.Platform = pl;
            tiket.WorkSpace = wp;
            tiket.User = user;
            tiket.Preorety = pr;
            tiket.DataPhone = dto.DataPhone;
            tiket.ResaultPhone = dto.ResaultPhone;
            tiket.DateSecondPhone = dto.DateSecondPhone;
            tiket.BugNumber = dto.BugNumber;
            tiket.BugTransfer = dto.BugTransfer;
            tiket.Mode = md;
            tiket.DataCreted = dto.DataCreted;
           

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

            var result = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TiketResponseDto
                {
                    Id = t.Oid,
                    Title = t.Title ?? string.Empty,
                    Description = t.Description ?? string.Empty,

                    AuthorId = t.Author != null ? t.Author.Oid : 0,
                    AuthorName = t.Author != null ? t.Author.Name : string.Empty,

                    CategoryName = t.Category != null ? t.Category.Name : string.Empty,

                    Phone = t.Phone ?? string.Empty,

                    CompanyId = t.Company != null ? t.Company.Oid : 0,
                    CompanyName = t.Company != null ? t.Company.Name : string.Empty,

                    SubCategoryName = t.SubCategory != null ? t.SubCategory.Name : string.Empty,

                    StateName = t.State != null ? t.State.Name : string.Empty,
                    StateId = t.State != null ? t.State.Oid : 0,

                    TypeTiketName = t.TypeTiket != null ? t.TypeTiket.Name : string.Empty,

                    PlatformId = t.Platform != null ? t.Platform.Oid : 0,
                    PlatformName = t.Platform != null ? t.Platform.Name : string.Empty,

                    WorkSpaceName = t.WorkSpace != null ? t.WorkSpace.Name : string.Empty,

                    UserId = t.User != null ? t.User.Oid : 0,
                    UserName = t.User != null ? t.User.Name : string.Empty,

                    PreorityName = t.Preorety != null ? t.Preorety.Name : string.Empty,

                    DataPhone = t.DataPhone,
                    ResaultPhone = t.ResaultPhone,
                    DateSecondPhone = t.DateSecondPhone,
                    BugNumber = t.BugNumber ?? string.Empty,
                    BugTransfer = t.BugTransfer,
                    ModeName = t.Mode != null ? t.Mode.Name : string.Empty,
                    DataCreted = t.DataCreted,
                    DataModefire = t.DataModefire,
                    DueDate = t.DueDate ?? DateTime.UtcNow,

                    // 📎 Файлы тикета
                    Files = t.Files.Select(f => new TiketFileDto
                    {
                        Id = f.Oid,
                        FileName = f.FileName,
                        FileUrl = f.FileUrl,
                        IsResponse = f.IsResponse
                    }).ToList(),

                    // 💬 Решения
                    Solution = t.Solutions.Select(s => new TiketSolutionDto
                    {
                        Id = s.Oid,
                        Author = s.Author != null ? s.Author.Oid : 0,
                        User = s.User != null ? s.User.Oid : 0,
                        MessageText = s.MessageText ?? string.Empty,
                        CreatedAt = s.CreatedAt,

                        // если EmailList хранится строкой через ;
                        EmailList = string.IsNullOrEmpty(s.EmailList)
                            ? new List<string> { s.Author.Email }
                            : s.EmailListParsed
                    }).ToList()
                }).ToList();
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



    private T GetOrFail<T>(UnitOfWork uow, int id, string entityName) where T : XPObject
    {
        var entity = uow.Query<T>().FirstOrDefault(e => e.Oid == id);
        if (entity == null)
            throw new KeyNotFoundException($"{entityName} не найден");
        return entity;
    }

}
