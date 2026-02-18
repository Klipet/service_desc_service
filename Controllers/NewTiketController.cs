
using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using static DevExpress.Data.Helpers.ExpressiveSortInfo;

[ApiController]
[Route("api/[controller]")]
public class NewTiketController : ControllerBase
{
    private readonly UnitOfWork _uow;
    public NewTiketController(UnitOfWork uow)
    {
        _uow = uow;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        try
        {

            var tikets = _uow.Query<NewTiket>().ToList();
            Console.WriteLine($"Найдено тикетов: {tikets.Count}");

            var result = tikets.Select(t => new NewTiketDto
            {
                Id = t.Id,    
                Title = t.Title,
                Description = t.Description,
                Author = t.Author,
                Category = t.Category,
                Phone = t.Phone,
                Company = t.Company,
                SubCategory = t.SubCategory,
                State = t.State,
                TypeTiket = t.TypeTiket,
                Platform = t.Platform,
                WorkSpaceName = t.WorkSpace.Name ,
                UserId = t.User?.Oid ?? 0,       
                UserName = t.User?.Name ?? "",
                Preorety = t.Preorety,
                DataPhone = t.DataPhone,
                ResaultPhone = t.ResaultPhone,
                DateSecondPhone = t.DateSecondPhone,
                BugNumber = t.BugNumber,
                BugTransfer = t.BugTransfer,
                Mode = t.Mode,
                DataCreted = t.DataCreted
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка: {ex.Message}");
        }
    }

    [HttpPost]
    public IActionResult Create([FromBody] NewTiketDto model)
    {
        using var uow = MyXPO.GetNewUnitOfWork();
        if (model == null)
            return BadRequest("Data is null");
        Console.WriteLine($"UserId из запроса: {model.UserId}");
        var user = uow.Query<User>().FirstOrDefault(u => u.Oid == model.UserId);
        if (user == null)
            return NotFound("Пользователь не найден");


        var wp = uow.Query<WorkSpace>().FirstOrDefault(u => u.Oid == model.WorkSpaceId);
        if (wp == null)
            return NotFound("Пользователь не найден");


        var ticket = new NewTiket(uow)
        {
            Title = model.Title,
            Description = model.Description,
            Author = model.Author,
            Category = model.Category,
            Phone = model.Phone,
            Company = model.Company,
            SubCategory = model.SubCategory,
            State = model.State,
            TypeTiket = model.TypeTiket,
            Platform = model.Platform,
            WorkSpace = wp,
            User = user,
            Preorety = model.Preorety,
            DataPhone = model.DataPhone,
            ResaultPhone = model.ResaultPhone,
            DateSecondPhone = model.DateSecondPhone,
            BugNumber = model.BugNumber,
            BugTransfer = model.BugTransfer,
            Mode = model.Mode,
            DataCreted = DateTime.UtcNow
        };
        try
        {
            uow.CommitChanges();
        }
        catch (Exception ex) {
            return StatusCode(500, $"Error saving to database: {ex.Message}");
        }
        var resault = new NewTiketDto
        {
            Title = ticket.Title,
            Description = ticket.Description,
            Author = ticket.Author,
            Category = ticket.Category,
            Phone = ticket.Phone,
            Company = ticket.Company,
            SubCategory = ticket.SubCategory,
            State = ticket.State,
            TypeTiket = ticket.TypeTiket,
            Platform = ticket.Platform,
            WorkSpaceName = ticket.WorkSpace.Name,
            UserName = user.Name,
            UserId = user.Oid,
            Preorety = ticket.Preorety,
            DataPhone = ticket.DataPhone,
            ResaultPhone = ticket.ResaultPhone,
            DateSecondPhone = ticket.DateSecondPhone,
            BugNumber = ticket.BugNumber,
            BugTransfer = ticket.BugTransfer,
            Mode = ticket.Mode,
            DataCreted = DateTime.UtcNow
        };
     //   return CreatedAtAction("GetTiketById", new { id = ticket.Id }, resault);
         return Ok(resault);
    }

    [HttpGet("{id}", Name = "GetTiketById")]
    public IActionResult GetById(int id)
    {
        var tiket = _uow.GetObjectByKey<NewTiket>(id);
        if(tiket == null) return NotFound();


        var user = _uow.GetObjectByKey<User>(tiket.User);
        if (user == null)
            return NotFound("Пользователь не найден");

        var wp = _uow.GetObjectByKey<WorkSpace>(tiket.WorkSpace);
        if (wp == null)
            return NotFound("Пользователь не найден");

        var tiketDto = new NewTiketDto
        {
            Id = tiket.Id, 
            Title = tiket.Title,
            Description = tiket.Description,
            Author = tiket.Author,
            Category = tiket.Category,
            Phone = tiket.Phone,
            Company = tiket.Company,
            SubCategory = tiket.SubCategory,
            State = tiket.State,
            TypeTiket = tiket.TypeTiket,
            Platform = tiket.Platform,
            WorkSpaceName = wp.Name,
            UserId = user.Oid,
            Preorety = tiket.Preorety,
            DataPhone = tiket.DataPhone,
            ResaultPhone = tiket.ResaultPhone,
            DateSecondPhone = tiket.DateSecondPhone,
            BugNumber = tiket.BugNumber,
            BugTransfer = tiket.BugTransfer,
            Mode = tiket.Mode,
            DataCreted = tiket.DataCreted

        };
                 
        return Ok(tiketDto);

    }
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] NewTiketDto dto)
    {
        try
        {
            
            var tiket = _uow.Query<NewTiket>().FirstOrDefault(t => t.Id == id);

            if (tiket == null)
                return NotFound($"Тикет с Id {id} не найден");

            // находим пользователя по UserId из dto
            var user = _uow.Query<User>().FirstOrDefault(u => u.Oid == dto.UserId);
            if (user == null)
                return NotFound("Пользователь не найден");
            var wp = _uow.Query<WorkSpace>().FirstOrDefault(u => u.Oid == dto.WorkSpaceId);
            if (wp == null)
                return NotFound("Пользователь не найден");

            tiket.Title = dto.Title;
            tiket.Description = dto.Description;
            tiket.Author = dto.Author;
            tiket.Category = dto.Category;
            tiket.Phone = dto.Phone;
            tiket.Company = dto.Company;
            tiket.SubCategory = dto.SubCategory;
            tiket.State = dto.State;
            tiket.TypeTiket = dto.TypeTiket;
            tiket.Platform = dto.Platform;
            tiket.WorkSpace = wp;
            tiket.User = user;
            tiket.Preorety = dto.Preorety;
            tiket.DataPhone = dto.DataPhone;
            tiket.ResaultPhone = dto.ResaultPhone;
            tiket.DateSecondPhone = dto.DateSecondPhone;
            tiket.BugNumber = dto.BugNumber;
            tiket.BugTransfer = dto.BugTransfer;
            tiket.Mode = dto.Mode;
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
            var ticket = _uow.Query<NewTiket>()
            .FirstOrDefault(t => t.Id == id);

     
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
}
