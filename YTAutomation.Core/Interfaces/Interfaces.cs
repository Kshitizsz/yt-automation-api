using YTAutomation.Core.DTOs;
using YTAutomation.Core.Entities;

namespace YTAutomation.Core.Interfaces;

// Repository Interfaces
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<bool> ExistsAsync(string email);
}

public interface IVideoJobRepository : IRepository<VideoJob>
{
    Task<IEnumerable<VideoJob>> GetByUserIdAsync(int userId);
    Task<IEnumerable<VideoJob>> GetPendingJobsAsync();
    Task<VideoJob?> GetWithAnalyticsAsync(int jobId);
}

public interface IMarketInsightRepository : IRepository<MarketInsight>
{
    Task<IEnumerable<MarketInsight>> GetByNicheAsync(string niche);
    Task<IEnumerable<MarketInsight>> GetLatestAsync(int count = 10);
}

public interface IScheduledPostRepository : IRepository<ScheduledPost>
{
    Task<IEnumerable<ScheduledPost>> GetDuePostsAsync();
    Task<IEnumerable<ScheduledPost>> GetByUserIdAsync(int userId);
}

// Service Interfaces (Application Layer)
public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
}

public interface IVideoJobService
{
    Task<VideoJobResponseDto> CreateJobAsync(int userId, CreateVideoJobDto dto);
    Task<VideoJobResponseDto?> GetJobAsync(int jobId);
    Task<IEnumerable<VideoJobResponseDto>> GetUserJobsAsync(int userId);
    Task ProcessJobAsync(int jobId);
    Task CancelJobAsync(int jobId);
}

public interface IMarketResearchService
{
    Task<MarketInsightDto> GetInsightsAsync(MarketResearchRequestDto dto);
    Task<IEnumerable<MarketInsightDto>> GetLatestTrendsAsync(int count = 10);
}

public interface ISEOService
{
    Task<SEOResponseDto> OptimizeAsync(SEORequestDto dto);
}

public interface IAnalyticsService
{
    Task<AnalyticsSummaryDto> GetSummaryAsync(int userId);
    Task SyncYouTubeAnalyticsAsync(int userId);
}

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetDashboardAsync(int userId);
}

// External AI Service Interfaces (Infrastructure Layer)
public interface IChatGPTService
{
    Task<string> GenerateScriptAsync(string topic, string niche);
    Task<string> OptimizeSEOAsync(string title, string description, string? keywords);
}

public interface IVideoGenerationService
{
    Task<string> GenerateVideoAsync(string script, string title);
}

public interface IVoiceoverService
{
    Task<string> GenerateVoiceoverAsync(string script);
}

public interface IThumbnailService
{
    Task<string> GenerateThumbnailAsync(string title, string niche);
}

public interface IGrokService
{
    Task<MarketInsightDto> AnalyzeTrendsAsync(string niche);
}

public interface IGeminiService
{
    Task<MarketInsightDto> AnalyzeTrendsAsync(string niche);
}

public interface IYouTubeService
{
    Task<string> UploadVideoAsync(int userId, VideoJob job);
    Task<VideoAnalyticsItemDto?> GetVideoAnalyticsAsync(string videoId);
}
