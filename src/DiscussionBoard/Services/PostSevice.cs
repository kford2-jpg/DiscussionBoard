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

        public async Task<Post?> GetAsync(int postId)
        {
            try
            {
                return await _context.Posts
                    .AsNoTracking()
                    .Include(p => p.ApplicationUser)
                    .FirstOrDefaultAsync(p =>
                        p.Id == postId &&
                        !p.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to retrieve post {PostId}.",
                    postId);

                throw;
            }
        }

        public async Task AddAsync(Post post)
        {
            try
            {
                _context.Posts.Add(post);
                await _context.SaveChangesAsync();
                _logger.LogInformation(
                    "Post {PostId} created in discussion thread {ThreadId} with parent {ParentPostId}",
                    post.Id,
                    post.DiscussionThreadId,
                    post.ParentPostId);
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

        public async Task UpdateAsync(Post post)
        {
            try
            {
                var existingPost = await _context.Posts
                    .FirstOrDefaultAsync(p =>
                        p.Id == post.Id &&
                        !p.IsDeleted);

                if (existingPost == null)
                {
                    throw new KeyNotFoundException(
                        $"Post {post.Id} was not found.");
                }

                existingPost.Title = post.Title;
                existingPost.Content = post.Content;

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to update post {PostId}.",
                    post.Id);

                throw;
            }
        }
    }
}
