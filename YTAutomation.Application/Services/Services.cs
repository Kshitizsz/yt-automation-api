using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using YTAutomation.Core.DTOs;
using YTAutomation.Core.Entities;
using YTAutomation.Core.Enums;
using YTAutomation.Core.Interfaces;
using BCrypt.Net;

namespace YTAutomation.Application.Services;

// ─── Auth Service ─────────────────────────────────────────────────────────────
public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IConfiguration _config;

    public AuthService(IUserRepository users, IConfiguration config)
    {
        _users = users;
        _config = config;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        if (await _users.ExistsAsync(dto.Email))
            throw new InvalidOperationException("Email already registered.");

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        await _users.AddAsync(user);
        return new AuthResponseDto(GenerateToken(user), user.Username, user.Email, user.Role);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _users.GetByEmailAsync(dto.Email)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        return new AuthResponseDto(GenerateToken(user), user.Username, user.Email, user.Role);
    }

    private string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

// ─── Video Job Service ────────────────────────────────────────────────────────
public class VideoJobService : IVideoJobService
{
    private readonly IVideoJobRepository _jobs;
    private readonly IScheduledPostRepository _scheduledPosts;
    private readonly IChatGPTService _chatGPT;
    private readonly IVideoGenerationService _videoGen;
    private readonly IVoiceoverService _voiceover;
    private readonly IThumbnailService _thumbnail;
    private readonly ISEOService _seo;

    public VideoJobService(
        IVideoJobRepository jobs,
        IScheduledPostRepository scheduledPosts,
        IChatGPTService chatGPT,
        IVideoGenerationService videoGen,
        IVoiceoverService voiceover,
        IThumbnailService thumbnail,
        ISEOService seo)
    {
        _jobs = jobs;
        _scheduledPosts = scheduledPosts;
        _chatGPT = chatGPT;
        _videoGen = videoGen;
        _voiceover = voiceover;
        _thumbnail = thumbnail;
        _seo = seo;
    }

    public async Task<VideoJobResponseDto> CreateJobAsync(int userId, CreateVideoJobDto dto)
    {
        var job = new VideoJob
        {
            UserId = userId,
            Topic = dto.Topic,
            NicheCategory = dto.NicheCategory,
            AIModel = dto.AIModel ?? "gpt-4o",
            Status = VideoJobStatus.Pending
        };

        await _jobs.AddAsync(job);

        if (dto.ScheduleAt.HasValue)
        {
            var scheduledPost = new ScheduledPost
            {
                UserId = userId,
                VideoJobId = job.Id,
                ScheduledAt = dto.ScheduleAt.Value
            };
            await _scheduledPosts.AddAsync(scheduledPost);
        }

        return MapToDto(job);
    }

    public async Task ProcessJobAsync(int jobId)
    {
        var job = await _jobs.GetByIdAsync(jobId);
        if (job == null) return;

        try
        {
            // Step 1: Generate Script
            job.Status = VideoJobStatus.GeneratingScript;
            await _jobs.UpdateAsync(job);
            job.Script = await _chatGPT.GenerateScriptAsync(job.Topic, job.NicheCategory ?? "general");

            // Step 2: Generate Voiceover
            job.Status = VideoJobStatus.GeneratingVoiceover;
            await _jobs.UpdateAsync(job);
            job.VoiceoverUrl = await _voiceover.GenerateVoiceoverAsync(job.Script);

            // Step 3: Generate Video
            job.Status = VideoJobStatus.GeneratingVideo;
            await _jobs.UpdateAsync(job);
            job.VideoUrl = await _videoGen.GenerateVideoAsync(job.Script, job.Topic);

            // Step 4: Generate Thumbnail
            job.Status = VideoJobStatus.GeneratingThumbnail;
            await _jobs.UpdateAsync(job);
            job.ThumbnailUrl = await _thumbnail.GenerateThumbnailAsync(job.Topic, job.NicheCategory ?? "general");

            // Step 5: SEO Optimization
            job.Status = VideoJobStatus.OptimizingSEO;
            await _jobs.UpdateAsync(job);
            var seoResult = await _seo.OptimizeAsync(new SEORequestDto(job.Topic, job.Script[..Math.Min(200, job.Script.Length)]));
            job.Title = seoResult.OptimizedTitle;
            job.Description = seoResult.OptimizedDescription;
            job.Tags = JsonSerializer.Serialize(seoResult.Tags);

            job.Status = VideoJobStatus.ReadyToPublish;
            await _jobs.UpdateAsync(job);
        }
        catch (Exception ex)
        {
            job.Status = VideoJobStatus.Failed;
            job.ErrorMessage = ex.Message;
            await _jobs.UpdateAsync(job);
        }
    }

    public async Task<VideoJobResponseDto?> GetJobAsync(int jobId)
    {
        var job = await _jobs.GetByIdAsync(jobId);
        return job == null ? null : MapToDto(job);
    }

    public async Task<IEnumerable<VideoJobResponseDto>> GetUserJobsAsync(int userId)
    {
        var jobs = await _jobs.GetByUserIdAsync(userId);
        return jobs.Select(MapToDto);
    }

    public async Task CancelJobAsync(int jobId)
    {
        var job = await _jobs.GetByIdAsync(jobId);
        if (job != null)
        {
            job.Status = VideoJobStatus.Cancelled;
            await _jobs.UpdateAsync(job);
        }
    }

    private static VideoJobResponseDto MapToDto(VideoJob j) => new(
        j.Id, j.Topic, j.Title, j.Description, j.Tags,
        j.ThumbnailUrl, j.VideoUrl, j.VoiceoverUrl, j.YoutubeVideoId,
        j.Status, j.ErrorMessage, j.NicheCategory ?? "", j.CreatedAt
    );
}

// ─── Market Research Service ──────────────────────────────────────────────────
public class MarketResearchService : IMarketResearchService
{
    private readonly IGrokService _grok;
    private readonly IGeminiService _gemini;
    private readonly IMarketInsightRepository _insights;

    public MarketResearchService(IGrokService grok, IGeminiService gemini, IMarketInsightRepository insights)
    {
        _grok = grok;
        _gemini = gemini;
        _insights = insights;
    }

    public async Task<MarketInsightDto> GetInsightsAsync(MarketResearchRequestDto dto)
    {
        var insight = dto.AISource.ToLower() == "grok"
            ? await _grok.AnalyzeTrendsAsync(dto.NicheCategory)
            : await _gemini.AnalyzeTrendsAsync(dto.NicheCategory);

        // Persist to DB
        var entity = new MarketInsight
        {
            NicheCategory = dto.NicheCategory,
            TrendingTopics = JsonSerializer.Serialize(insight.TrendingTopics),
            AnalysisSummary = insight.AnalysisSummary,
            AISource = dto.AISource,
            TrendScore = insight.TrendScore
        };
        await _insights.AddAsync(entity);

        return insight with { Id = entity.Id };
    }

    public async Task<IEnumerable<MarketInsightDto>> GetLatestTrendsAsync(int count = 10)
    {
        var items = await _insights.GetLatestAsync(count);
        return items.Select(m => new MarketInsightDto(
            m.Id, m.NicheCategory,
            JsonSerializer.Deserialize<List<string>>(m.TrendingTopics) ?? new(),
            m.AnalysisSummary, m.AISource, m.TrendScore, m.FetchedAt
        ));
    }
}

// ─── SEO Service ─────────────────────────────────────────────────────────────
public class SEOService : ISEOService
{
    private readonly IChatGPTService _chatGPT;

    public SEOService(IChatGPTService chatGPT) => _chatGPT = chatGPT;

    public async Task<SEOResponseDto> OptimizeAsync(SEORequestDto dto)
    {
        var raw = await _chatGPT.OptimizeSEOAsync(dto.Title, dto.Description, dto.Keywords);
        try
        {
            var clean = raw.Trim();
            if (clean.Contains("```json")) clean = clean[(clean.IndexOf("```json") + 7)..];
            if (clean.Contains("```")) clean = clean[..clean.LastIndexOf("```")];
            var data = JsonSerializer.Deserialize<JsonElement>(clean.Trim());

            var title = data.GetProperty("optimizedTitle").GetString() ?? dto.Title;
            var desc = data.GetProperty("optimizedDescription").GetString() ?? dto.Description;
            var tags = data.GetProperty("tags").EnumerateArray().Select(t => t.GetString() ?? "").ToList();
            var score = data.GetProperty("seoScore").GetDouble();

            return new SEOResponseDto(title, desc, tags, score);
        }
        catch
        {
            return new SEOResponseDto(dto.Title, dto.Description, new List<string>(), 50);
        }
    }
}

// ─── Analytics Service ────────────────────────────────────────────────────────
public class AnalyticsService : IAnalyticsService
{
    private readonly IVideoJobRepository _jobs;

    public AnalyticsService(IVideoJobRepository jobs) => _jobs = jobs;

    public async Task<AnalyticsSummaryDto> GetSummaryAsync(int userId)
    {
        var jobs = (await _jobs.GetByUserIdAsync(userId)).ToList();
        var published = jobs.Where(j => j.Status == VideoJobStatus.Published).ToList();

        return new AnalyticsSummaryDto(
            TotalViews: published.Sum(j => j.Analytics?.Views ?? 0),
            TotalLikes: published.Sum(j => j.Analytics?.Likes ?? 0),
            TotalComments: published.Sum(j => j.Analytics?.Comments ?? 0),
            Subscribers: 0,
            TotalVideos: jobs.Count,
            PublishedVideos: published.Count,
            RecentVideos: published.Take(5).Select(j => new VideoAnalyticsItemDto(
                j.Title ?? j.Topic,
                j.Analytics?.Views ?? 0,
                j.Analytics?.Likes ?? 0,
                j.Analytics?.Comments ?? 0,
                j.Analytics?.WatchTimeMinutes ?? 0,
                j.CreatedAt
            )).ToList()
        );
    }

    public Task SyncYouTubeAnalyticsAsync(int userId) => Task.CompletedTask; // Implement with YouTube Data API
}

// ─── Dashboard Service ────────────────────────────────────────────────────────
public class DashboardService : IDashboardService
{
    private readonly IVideoJobRepository _jobs;
    private readonly IMarketResearchService _market;

    public DashboardService(IVideoJobRepository jobs, IMarketResearchService market)
    {
        _jobs = jobs;
        _market = market;
    }

    public async Task<DashboardSummaryDto> GetDashboardAsync(int userId)
    {
        var jobs = (await _jobs.GetByUserIdAsync(userId)).ToList();
        var trends = await _market.GetLatestTrendsAsync(5);

        return new DashboardSummaryDto(
            TotalJobs: jobs.Count,
            CompletedJobs: jobs.Count(j => j.Status == VideoJobStatus.Published || j.Status == VideoJobStatus.ReadyToPublish),
            FailedJobs: jobs.Count(j => j.Status == VideoJobStatus.Failed),
            ScheduledJobs: jobs.Count(j => j.Status == VideoJobStatus.Pending),
            TotalViews: jobs.Sum(j => j.Analytics?.Views ?? 0),
            RecentJobs: jobs.Take(5).Select(j => new VideoJobResponseDto(
                j.Id, j.Topic, j.Title, j.Description, j.Tags,
                j.ThumbnailUrl, j.VideoUrl, j.VoiceoverUrl, j.YoutubeVideoId,
                j.Status, j.ErrorMessage, j.NicheCategory ?? "", j.CreatedAt
            )).ToList(),
            TopTrends: trends.ToList()
        );
    }
}
