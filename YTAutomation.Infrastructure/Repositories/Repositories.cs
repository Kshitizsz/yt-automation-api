using Microsoft.EntityFrameworkCore;
using YTAutomation.Core.Entities;
using YTAutomation.Core.Interfaces;
using YTAutomation.Infrastructure.Data;

namespace YTAutomation.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

    public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity != null)
        {
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email) =>
        await _dbSet.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<bool> ExistsAsync(string email) =>
        await _dbSet.AnyAsync(u => u.Email == email);
}

public class VideoJobRepository : Repository<VideoJob>, IVideoJobRepository
{
    public VideoJobRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<VideoJob>> GetByUserIdAsync(int userId) =>
        await _dbSet.Where(v => v.UserId == userId).OrderByDescending(v => v.CreatedAt).ToListAsync();

    public async Task<IEnumerable<VideoJob>> GetPendingJobsAsync() =>
        await _dbSet.Where(v => v.Status == Core.Enums.VideoJobStatus.Pending).ToListAsync();

    public async Task<VideoJob?> GetWithAnalyticsAsync(int jobId) =>
        await _dbSet.Include(v => v.Analytics).FirstOrDefaultAsync(v => v.Id == jobId);
}

public class MarketInsightRepository : Repository<MarketInsight>, IMarketInsightRepository
{
    public MarketInsightRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<MarketInsight>> GetByNicheAsync(string niche) =>
        await _dbSet.Where(m => m.NicheCategory == niche).OrderByDescending(m => m.FetchedAt).ToListAsync();

    public async Task<IEnumerable<MarketInsight>> GetLatestAsync(int count = 10) =>
        await _dbSet.OrderByDescending(m => m.FetchedAt).Take(count).ToListAsync();
}

public class ScheduledPostRepository : Repository<ScheduledPost>, IScheduledPostRepository
{
    public ScheduledPostRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<ScheduledPost>> GetDuePostsAsync() =>
        await _dbSet.Include(s => s.VideoJob)
            .Where(s => s.Status == Core.Enums.ScheduledPostStatus.Scheduled && s.ScheduledAt <= DateTime.UtcNow)
            .ToListAsync();

    public async Task<IEnumerable<ScheduledPost>> GetByUserIdAsync(int userId) =>
        await _dbSet.Include(s => s.VideoJob)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.ScheduledAt)
            .ToListAsync();
}
