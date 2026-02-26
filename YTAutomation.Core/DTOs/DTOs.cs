using YTAutomation.Core.Enums;

namespace YTAutomation.Core.DTOs;

// Auth
public record RegisterDto(string Username, string Email, string Password);
public record LoginDto(string Email, string Password);
public record AuthResponseDto(string Token, string Username, string Email, string Role);

// Video Job
public record CreateVideoJobDto(
    string Topic,
    string NicheCategory,
    string? AIModel = "gpt-4o",
    bool AutoPublish = false,
    DateTime? ScheduleAt = null
);

public record VideoJobResponseDto(
    int Id,
    string Topic,
    string? Title,
    string? Description,
    string? Tags,
    string? ThumbnailUrl,
    string? VideoUrl,
    string? VoiceoverUrl,
    string? YoutubeVideoId,
    VideoJobStatus Status,
    string? ErrorMessage,
    string NicheCategory,
    DateTime CreatedAt
);

public record VideoJobProgressDto(int JobId, VideoJobStatus Status, int ProgressPercent, string Message);

// Market Research
public record MarketInsightDto(
    int Id,
    string NicheCategory,
    List<string> TrendingTopics,
    string AnalysisSummary,
    string AISource,
    double TrendScore,
    DateTime FetchedAt
);

public record MarketResearchRequestDto(string NicheCategory, string AISource = "gemini");

// SEO
public record SEORequestDto(string Title, string Description, string? Keywords = null);
public record SEOResponseDto(string OptimizedTitle, string OptimizedDescription, List<string> Tags, double SEOScore);

// Analytics
public record AnalyticsSummaryDto(
    long TotalViews,
    long TotalLikes,
    long TotalComments,
    long Subscribers,
    int TotalVideos,
    int PublishedVideos,
    List<VideoAnalyticsItemDto> RecentVideos
);

public record VideoAnalyticsItemDto(
    string Title,
    long Views,
    long Likes,
    long Comments,
    double WatchTimeMinutes,
    DateTime PublishedAt
);

// Dashboard
public record DashboardSummaryDto(
    int TotalJobs,
    int CompletedJobs,
    int FailedJobs,
    int ScheduledJobs,
    long TotalViews,
    List<VideoJobResponseDto> RecentJobs,
    List<MarketInsightDto> TopTrends
);
