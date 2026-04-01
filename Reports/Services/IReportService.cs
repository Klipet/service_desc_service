public interface IReportService
{
    Task<ReportResultDto> BuildAsync(ReportRequestDto request);
    Task<byte[]> ExportToExcelAsync(ReportResultDto result);
}

public interface IReportRepository
{
    Task<int> SaveConfigAsync(ReportRequestDto request);
    Task<List<ReportConfigDto>> GetAllAsync();
    Task<ReportConfigDto?> GetByIdAsync(int id);
    Task<bool> DeleteAsync(int id);
}
