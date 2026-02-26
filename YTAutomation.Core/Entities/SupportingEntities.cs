using YTAutomation.Core.Enums;

namespace YTAutomation.Core.Entities;

public class ScheduledPost : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int VideoJobId { get; set; }
    public VideoJob VideoJob { get; set; } = null!;
    public DateTime ScheduledAt { get; set; }
    public ScheduledPostStatus Status { get; set; } = ScheduledPostStatus.Scheduled;
    public string? PublishResult { get; set; }
}

public class MarketInsight : BaseEntity
{
    public string NicheCategory { get; set; } = string.Empty;
    public string TrendingTopics { get; set; } = string.Empty; // JSON array
    public string AnalysisSummary { get; set; } = string.Empty;
    public string AISource { get; set; } = "grok"; // grok or gemini
    public DateTime FetchedAt { get; set; } = DateTime.UtcNow;
    public double TrendScore { get; set; }
}

public class VideoAnalytics : BaseEntity
{
    public int VideoJobId { get; set; }
    public VideoJob VideoJob { get; set; } = null!;
    public long Views { get; set; }
    public long Likes { get; set; }
    public long Comments { get; set; }
    public long Subscribers { get; set; }
    public double WatchTimeMinutes { get; set; }
    public DateTime LastSyncedAt { get; set; } = DateTime.UtcNow;
}
