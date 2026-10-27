using DiscussionBoard.Data;
using DiscussionBoard.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace DiscussionBoard.Services
{
    public class PostService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PostService> _logger;

        public PostService(ApplicationDbContext context)
            : this(context, NullLogger<PostService>.Instance)
        {
        }

        public PostService(
            ApplicationDbContext context,
            ILogger<PostService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Post>> GetForThreadAsync(int threadId)
        {
            try
            {
                var posts = await _context.Posts
                    .AsNoTracking()
                    .Where(p =>
                        p.DiscussionThreadId == threadId &&
                        !p.IsDeleted)
                    .Include(p => p.ApplicationUser)
                    .ToListAsync();

                return posts
                    .OrderBy(p => p.CreatedAt)
                    .ThenBy(p => p.Id)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to retrieve posts for thread {ThreadId}.",
                    threadId);

                throw;
            }
        }

        public async Task AddAsync(Post post)
        {
            try
            {
                _context.Posts.Add(post);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to add a post to thread {ThreadId}.",
                    post.DiscussionThreadId);

                throw;
            }
        }
    }
}
