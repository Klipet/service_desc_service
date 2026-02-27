using DevExpress.Xpo;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class EmailTemplateController : ControllerBase
{
    private readonly UnitOfWork _uow;
    public EmailTemplateController(UnitOfWork uow)
    {
        _uow = uow;
    }

    [HttpPost("NewTempalte")]
    public IActionResult Create([FromBody] EmailTemplateDto model)
    {
        if (model == null) return BadRequest("Data is null");

        var state = _uow.Query<State>().FirstOrDefault(s => s.Oid == model.StateOid) ??
            throw new KeyNotFoundException("State не найден");
        try
        {
            var resault = new EmailTemplate(_uow)
            {
                SubjectTemplate = model.SubjectTemplate,
                BodyHtmlTemplate = model.BodyHtmlTemplate,
                IsActive = model.IsActive,
                IsHtml = model.IsHtml,
                State = state,
            };
            _uow.CommitChanges();
            return Ok(resault);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error saving to database: {ex.Message}");
        }
    }

    [HttpGet("GetAllTemplate")]
    public IActionResult GetAll() {
        try
        {
            var tempate = _uow.Query<EmailTemplate>().ToList();

            var resault = tempate.Select(t => new EmailTemplateDto
            {
                Oid = t.Oid,
                SubjectTemplate = t.SubjectTemplate,
                BodyHtmlTemplate = t.BodyHtmlTemplate,
                IsActive = t.IsActive,
                IsHtml = t.IsHtml,
                StateName = t.State.Name,
                StateOid = t.State.Oid,
            }
            ).ToList();

            return Ok(resault);
        }
        catch (Exception ex) {
            return StatusCode(500, $"Ошибка: {ex.Message}");
        }
    }

    [HttpGet("GetTempalteById")]
    public IActionResult GetById([FromQuery] int id) 
    {
        var tempalte = _uow.GetObjectByKey<EmailTemplate>(id);
        if(tempalte == null) return NotFound();

        var state = _uow.GetObjectByKey<State>(tempalte.State.Oid) ?? throw new KeyNotFoundException("State не найден");

        var response = new EmailTemplateDto
        {
            Oid = tempalte.Oid,
            SubjectTemplate = tempalte.SubjectTemplate,
            BodyHtmlTemplate= tempalte.BodyHtmlTemplate,
            IsActive = tempalte.IsActive,
            IsHtml = tempalte.IsHtml,
            StateName = state.Name,
            StateOid = state.Oid,
        };
        return Ok(response);

    }

    [HttpDelete("DeleteTemplate")]
    public IActionResult Delete([FromQuery] int id)
    {
        try
        {
            // Ищем через сессию явно
            var template = _uow.Query<EmailTemplate>()
            .FirstOrDefault(t => t.Oid == id);


            if (template == null)
                return NotFound($"Тикет с id={id} не найден");

            // Удаляем
            _uow.Delete(template);
            _uow.CommitChanges();

            return NoContent(); // 204
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка при удалении: {ex.Message}");
        }
    }

}
