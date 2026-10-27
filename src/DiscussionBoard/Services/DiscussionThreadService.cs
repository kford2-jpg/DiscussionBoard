using DiscussionBoard.Data;
using DiscussionBoard.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace DiscussionBoard.Services
{
    public class DiscussionThreadService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DiscussionThreadService> _logger;

        public DiscussionThreadService(ApplicationDbContext context)
            : this(context, NullLogger<DiscussionThreadService>.Instance)
        {
        }

        public DiscussionThreadService(
            ApplicationDbContext context,
            ILogger<DiscussionThreadService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<DiscussionThread?> GetAsync(int id)
        {
            try
            {
                return await _context.DiscussionThreads
                    .Include(t => t.ApplicationUser)
                    .Include(t => t.Forum)
                    .FirstOrDefaultAsync(
                        t => t.Id == id && !t.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to retrieve discussion thread {ThreadId}.",
                    id);

                throw;
            }
        }

        public async Task<List<DiscussionThread>> GetForForumAsync(
            int forumId)
        {
            try
            {
                var threads = await _context.DiscussionThreads
                    .Include(t => t.ApplicationUser)
                    .Where(t =>
                        t.ForumId == forumId &&
                        !t.IsDeleted)
                    .ToListAsync();

                return threads
                    .OrderByDescending(t => t.CreatedAt)
                    .ThenByDescending(t => t.Id)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to retrieve threads for forum {ForumId}.",
                    forumId);

                throw;
            }
        }

        public async Task AddAsync(DiscussionThread thread)
        {
            try
            {
                _context.DiscussionThreads.Add(thread);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to create a discussion thread.");

                throw;
            }
        }

        public async Task<bool> UpdateAsync(DiscussionThread thread)
        {
            try
            {
                var existing = await _context.DiscussionThreads
                    .FirstOrDefaultAsync(
                        t => t.Id == thread.Id && !t.IsDeleted);

                if (existing == null)
                    return false;

                existing.Title = thread.Title;
                existing.Content = thread.Content;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to update discussion thread {ThreadId}.",
                    thread.Id);

                throw;
            }
        }
    }
}
