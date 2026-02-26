using YTAutomation.Core.Enums;

namespace YTAutomation.Core.Entities;

public class VideoJob : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string Topic { get; set; } = string.Empty;
    public string? Script { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Tags { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? VideoUrl { get; set; }
    public string? VoiceoverUrl { get; set; }
    public string? YoutubeVideoId { get; set; }
    public VideoJobStatus Status { get; set; } = VideoJobStatus.Pending;
    public string? ErrorMessage { get; set; }
    public string? NicheCategory { get; set; }
    public string? AIModel { get; set; } = "gpt-4o";
    public int? ScheduledPostId { get; set; }
    public ScheduledPost? ScheduledPost { get; set; }
    public VideoAnalytics? Analytics { get; set; }
}
