
using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using static DevExpress.Data.Helpers.ExpressiveSortInfo;

[ApiController]
[Route("api/[controller]")]
public class TiketController : ControllerBase
{
    private readonly UnitOfWork _uow;
    public TiketController(UnitOfWork uow)
    {
        _uow = uow;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        try
        {

            var tikets = _uow.Query<Tiket>().ToList();
            Console.WriteLine($"Найдено тикетов: {tikets.Count}");

            var result = tikets.Select(t => new TiketDto
            {
                Id = t.Oid,    
                Title = t.Title,
                Description = t.Description,
                AuthorId = t.Author.Oid,
                AuthorName = t.Author.Name,
                CategoryName = t.Category.Name,
                Phone = t.Phone,
                CompanyId = t.Company.Oid,
                CompanyName = t.Company.Name,
                SubCategoryName = t.SubCategory.Name,
                StateName = t.State.Name,
                StateId = t.State.Oid,
                TypeTiketName = t.TypeTiket.Name,
                PlatformId = t.Platform.Oid,
                PlatformName = t.Platform.Name,
                WorkSpaceName = t.WorkSpace.Name ,
                UserId = t.User?.Oid ?? 0,       
                UserName = t.User?.Name ?? "",
                PreorityName = t.Preorety.Name,
                DataPhone = t.DataPhone,
                ResaultPhone = t.ResaultPhone,
                DateSecondPhone = t.DateSecondPhone,
                BugNumber = t.BugNumber,
                BugTransfer = t.BugTransfer,
                ModeName = t.Mode.Name,
                DataCreted = t.DataCreted,
                DataModefire = t.DataModefire,
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка: {ex.Message}");
        }
    }

    [HttpPost]
    public IActionResult Create([FromBody] TiketDto model)
    {
        using var uow = MyXPO.GetNewUnitOfWork();
        if (model == null) return BadRequest("Data is null");

        var user = GetOrFail<User>(uow, model.UserId, "User");
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
        var resault = new TiketDto
        {
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
            DataCreted = ticket.DataCreted,
        };
     //   return CreatedAtAction("GetTiketById", new { id = ticket.Id }, resault);
         return Ok(resault);
    }

    [HttpGet("{id}", Name = "GetTiketById")]
    public IActionResult GetById(int id)
    {
        var tiket = _uow.GetObjectByKey<Tiket>(id);
        if(tiket == null) return NotFound();


        var user = _uow.GetObjectByKey<User>(tiket.User) ?? throw new KeyNotFoundException("Пользователь не найден");
        var wp = _uow.GetObjectByKey<WorkSpace>(tiket.WorkSpace) ?? throw new KeyNotFoundException("WorkSpace не найден");
        var st = _uow.GetObjectByKey<State>(tiket.State) ?? throw new KeyNotFoundException("State не найден");
        var tt = _uow.GetObjectByKey<TiketType>(tiket.TypeTiket) ?? throw new KeyNotFoundException("TiketType не найден");
        var pr = _uow.GetObjectByKey<Preority>(tiket.Preorety) ?? throw new KeyNotFoundException("Preority не найден");
        var md = _uow.GetObjectByKey<Mode>(tiket.Mode) ?? throw new KeyNotFoundException("Mode не найден");
        var sc = _uow.GetObjectByKey<SubCategory>(tiket.SubCategory) ?? throw new KeyNotFoundException("SubCategory не найден");
        var cat = _uow.GetObjectByKey<Category>(tiket.Category) ?? throw new KeyNotFoundException("Category не найден");
        var au = _uow.GetObjectByKey<Author>(tiket.Author) ?? throw new KeyNotFoundException("Author не найден");
        var pl = _uow.GetObjectByKey<Platform>(tiket.Platform) ?? throw new KeyNotFoundException("Platform не найден");
        var com = _uow.GetObjectByKey<Company>(tiket.Company) ?? throw new KeyNotFoundException("Company не найден");

        var tiketDto = new TiketDto
        {
            Id = tiket.Oid, 
            Title = tiket.Title,
            Description = tiket.Description,
            AuthorName = au.Name,
            AuthorId = au.Oid,
            CategoryName = cat.Name,
            Phone = tiket.Phone,
            CompanyName = com.Name,
            CompanyId = com.Oid,
            SubCategoryName = sc.Name,
            StateName = st.Name,
            TypeTiketName = tt.Name,
            PlatformName = pl.Name,
            PlatformId = pl.Oid,
            WorkSpaceName = wp.Name,
            UserId = user.Oid,
            PreorityId = pr.Oid,
            DataPhone = tiket.DataPhone,
            ResaultPhone = tiket.ResaultPhone,
            DateSecondPhone = tiket.DateSecondPhone,
            BugNumber = tiket.BugNumber,
            BugTransfer = tiket.BugTransfer,
            ModeName = md.Name,
            DataCreted = tiket.DataCreted

        };
                 
        return Ok(tiketDto);

    }
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] TiketDto dto)
    {
        try
        {
            
            var tiket = _uow.Query<Tiket>().FirstOrDefault(t => t.Oid == id);

            if (tiket == null) return NotFound($"Тикет с Id {id} не найден");

            var user = GetOrFail<User>(_uow, tiket.User.Oid, "Пользователь");
            var wp = GetOrFail<WorkSpace>(_uow, tiket.WorkSpace.Oid, "WorkSpace");
            var st = GetOrFail<State>(_uow, tiket.State.Oid, "State");
            var tt = GetOrFail<TiketType>(_uow, tiket.TypeTiket.Oid, "TiketType");
            var pr = GetOrFail<Preority>(_uow, tiket.Preorety.Oid, "Preority");
            var md = GetOrFail<Mode>(_uow, tiket.Mode.Oid, "Mode");
            var sc = GetOrFail<SubCategory>(_uow, tiket.SubCategory.Oid, "SubCategory");
            var cat = GetOrFail<Category>(_uow, tiket.Category.Oid, "Category");
            var au = GetOrFail<Author>(_uow, tiket.Author.Oid, "Autor");
            var pl = GetOrFail<Platform>(_uow, tiket.Platform.Oid, "Platform");
            var com = GetOrFail<Company>(_uow, tiket.Company.Oid, "Company");

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

            return Ok($"Tiket {id} update");
        }
        catch (Exception ex)
        {
            return StatusCode(ex.HResult, $"Ошибка при обновлении: {ex.Message}");
        }
    }




    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
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

    private T GetOrFail<T>(UnitOfWork uow, int id, string entityName) where T : XPObject
    {
        var entity = uow.Query<T>().FirstOrDefault(e => e.Oid == id);
        if (entity == null)
            throw new KeyNotFoundException($"{entityName} не найден");
        return entity;
    }

}
