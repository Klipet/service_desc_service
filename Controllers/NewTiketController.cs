
using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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
            using var uow = MyXPO.GetNewUnitOfWork();

            var tikets = _uow.Query<NewTiket>().ToList();

            Console.WriteLine($"Найдено тикетов: {tikets.Count}");

            var result = tikets.Select(t => new NewTiketDto
            {
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
                WorkSpace = t.WorkSpace,
                User = t.User,
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
        if (model == null)
            return BadRequest("Data is null");

        var ticket = new NewTiket(_uow)
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
            WorkSpace = model.WorkSpace,
            User = model.User,
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
            _uow.CommitChanges();
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
            WorkSpace = ticket.WorkSpace,
            User = ticket.User,
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

        var tiketDto = new NewTiketDto
        {
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
            WorkSpace = tiket.WorkSpace,
            User = tiket.User,
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





    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        try
        {
            // Ищем через сессию явно
            var ticket = _uow.Query<NewTiket>()
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
}
