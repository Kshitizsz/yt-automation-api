using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YTAutomation.Core.DTOs;
using YTAutomation.Core.Interfaces;

namespace YTAutomation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        try { return Ok(await _auth.RegisterAsync(dto)); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try { return Ok(await _auth.LoginAsync(dto)); }
        catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VideoJobsController : ControllerBase
{
    private readonly IVideoJobService _service;
    public VideoJobsController(IVideoJobService service) => _service = service;

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetUserJobsAsync(UserId));

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var job = await _service.GetJobAsync(id);
        return job == null ? NotFound() : Ok(job);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVideoJobDto dto)
    {
        var job = await _service.CreateJobAsync(UserId, dto);
        // Fire-and-forget processing
        _ = Task.Run(() => _service.ProcessJobAsync(job.Id));
        return CreatedAtAction(nameof(Get), new { id = job.Id }, job);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Cancel(int id)
    {
        await _service.CancelJobAsync(id);
        return NoContent();
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MarketResearchController : ControllerBase
{
    private readonly IMarketResearchService _service;
    public MarketResearchController(IMarketResearchService service) => _service = service;

    [HttpPost("analyze")]
    public async Task<IActionResult> Analyze([FromBody] MarketResearchRequestDto dto)
        => Ok(await _service.GetInsightsAsync(dto));

    [HttpGet("trends")]
    public async Task<IActionResult> GetTrends([FromQuery] int count = 10)
        => Ok(await _service.GetLatestTrendsAsync(count));
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SEOController : ControllerBase
{
    private readonly ISEOService _service;
    public SEOController(ISEOService service) => _service = service;

    [HttpPost("optimize")]
    public async Task<IActionResult> Optimize([FromBody] SEORequestDto dto)
        => Ok(await _service.OptimizeAsync(dto));
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _service;
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    public AnalyticsController(IAnalyticsService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetSummary() => Ok(await _service.GetSummaryAsync(UserId));

    [HttpPost("sync")]
    public async Task<IActionResult> Sync()
    {
        await _service.SyncYouTubeAnalyticsAsync(UserId);
        return Ok(new { message = "Analytics synced" });
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    public DashboardController(IDashboardService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await _service.GetDashboardAsync(UserId));
}
