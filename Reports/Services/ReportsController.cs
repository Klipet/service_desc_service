using Microsoft.AspNetCore.Mvc;
using System.Text;

[ApiController]
[Route("[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly IReportRepository _reportRepository;

    public ReportsController(
        IReportService reportService,
        IReportRepository reportRepository)
    {
        _reportService = reportService;
        _reportRepository = reportRepository;
    }

    // POST api/reports/run
    [HttpPost("run")]
    [ProducesResponseType(typeof(ReportResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Run([FromBody] ReportRequestDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _reportService.BuildAsync(request);
        return Ok(result);
    }

    // POST api/reports/save
    [HttpPost("save")]
    [ProducesResponseType(typeof(ReportResultDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Save([FromBody] ReportRequestDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var configId = await _reportRepository.SaveConfigAsync(request);
        var result = await _reportService.BuildAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = configId }, result);
    }

    // GET api/reports
    [HttpGet]
    [ProducesResponseType(typeof(List<ReportConfigDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var resault = await _reportRepository.GetAllAsync();

        return Ok(resault);
    }

    // GET api/reports/{id}
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReportConfigDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var config = await _reportRepository.GetByIdAsync(id);
        if (config == null) return NotFound();
        return Ok(config);
    }

    // POST api/reports/{id}/run
    [HttpPost("{id:int}/run")]
    [ProducesResponseType(typeof(ReportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RunSaved(int id)
    {
        var config = await _reportRepository.GetByIdAsync(id);
        if (config == null) return NotFound();

        var result = await _reportService.BuildAsync(new ReportRequestDto
        {
            Name = config.Name,
            DateFrom = config.DateFrom,
            DateTo = config.DateTo,
            GroupBy = config.GroupBy,
        });

        return Ok(result);
    }

    // DELETE api/reports/{id}
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _reportRepository.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }

    // GET api/reports/{id}/export?format=excel
    [HttpGet("{id:int}/export")]
    public async Task<IActionResult> Export(int id, [FromQuery] string format = "excel")
    {
        var config = await _reportRepository.GetByIdAsync(id);
        if (config == null) return NotFound();

        var result = await _reportService.BuildAsync(new ReportRequestDto
        {
            Name = config.Name,
            DateFrom = config.DateFrom,
            DateTo = config.DateTo,
            GroupBy = config.GroupBy,
        });

        if (format.Equals("excel", StringComparison.OrdinalIgnoreCase))
        {
            var bytes = await _reportService.ExportToExcelAsync(result);
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{result.ReportName}_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        if (format.Equals("csv", StringComparison.OrdinalIgnoreCase))
        {
            var sb = new StringBuilder();
            sb.AppendLine("Группа;Количество");
            foreach (var row in result.Rows)
                sb.AppendLine($"{row.Label};{row.Count}");
            return File(Encoding.UTF8.GetBytes(sb.ToString()),
                "text/csv",
                $"{result.ReportName}_{DateTime.Now:yyyyMMdd}.csv");
        }

        return BadRequest(new { error = "Поддерживаются форматы: excel, csv" });
    }
}
