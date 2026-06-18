using TheUnraveller.Service.DTOs;

namespace TheUnraveller.Service.Interfaces;

public interface IProgressService
{
    Task<SkillMapDto> GetSkillMapAsync(int userId);
    Task<List<PortfolioEntryDto>> GetPortfolioAsync(int userId);
    Task<WeeklyReportDto> GetWeeklyReportAsync(int userId);
}
