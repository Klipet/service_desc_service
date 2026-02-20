using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class TiketLogController: ControllerBase
{
  //  private readonly UnitOfWork _uow;
 //   public TiketLogController(UnitOfWork uow){_uow = uow}

    private readonly Session _session;
    public TiketLogController(Session session)
    {
        _session = session;
    }
    [HttpGet]
    public IActionResult GetAll()
    {
        try
        {
            var logs = new XPCollection<TiketLog>(_session);
            return Ok(logs);
            /*        var tikets = _uow.Query<TiketLog>().ToList();
                    Console.WriteLine($"Найдено изменений: {tikets.Count}");

                    var result = tikets.Select(t => new TiketLogDto
                    {
                        TiketId = t.Oid,
                        Action = t.Action,
                        User = t.User,

                        Title = t.Title,
                        Description = t.Description,
                        Company = t.Company,
                        Category = t.Category,
                        SubCategory = t.SubCategory,
                        State = t.State,
                        TypeTiket = t.TypeTiket,
                        Author = t.Author,
                        Platform = t.Platform,
                        WorkSpace = t.WorkSpace,
                        Preority = t.Preority,
                        Phone = t.Phone,
                        DataPhone = t.DataPhone,
                        ResaultPhone = t.ResaultPhone,
                        DateSecondPhone = t.DateSecondPhone,
                        BugNumber = t.BugNumber,
                        Mode = t.Mode,
                        BugTransfer = t.BugTransfer,
                        DataModefire = t.DataModefire,
                    }).ToList();

                    return Ok(result);
            */

        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка: {ex.Message}");
        }
    }
    [HttpGet("{tiketId}")]
    public IActionResult GetByTiketId(int tiketId)
    {
        var logs = new XPCollection<TiketLog>(
            _session,
            CriteriaOperator.Parse("TiketId = ?", tiketId)
        );
        return Ok(logs);
    }

}


