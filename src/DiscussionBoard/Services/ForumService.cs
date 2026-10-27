using DiscussionBoard.Data;
using DiscussionBoard.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace DiscussionBoard.Services
{
    public class ForumService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ForumService> _logger;

        public ForumService(ApplicationDbContext context)
            : this(context, NullLogger<ForumService>.Instance)
        {
        }

        public ForumService(
            ApplicationDbContext context,
            ILogger<ForumService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Forum>> GetAllAsync()
        {
            try
            {
                return await _context.Forums
                    .Where(f => !f.IsDeleted)
                    .OrderBy(f => f.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve forums.");
                throw;
            }
        }
    }
}
