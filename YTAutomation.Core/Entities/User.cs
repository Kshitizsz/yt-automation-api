namespace YTAutomation.Core.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? YoutubeChannelId { get; set; }
    public string? YoutubeRefreshToken { get; set; }
    public string Role { get; set; } = "User";
    public ICollection<VideoJob> VideoJobs { get; set; } = new List<VideoJob>();
    public ICollection<ScheduledPost> ScheduledPosts { get; set; } = new List<ScheduledPost>();
}
