
using System.Security.Claims;
using DiscussionBoard.Models;
using DiscussionBoard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DiscussionBoard.Pages.Discussions;

public class DetailsModel : PageModel
{
    private readonly DiscussionThreadService _threads;
    private readonly PostService _posts;

    public DiscussionThread? Thread { get; private set; }

    public List<PostTreeBuilder.Node> PostTree { get; private set; } = [];

    [BindProperty]
    public PostInputModel Input { get; set; } = new();

    public DetailsModel(
        DiscussionThreadService threads,
        PostService posts)
    {
        _threads = threads;
        _posts = posts;
    }

    public async Task<IActionResult> OnGetAsync(int threadId)
    {
        if (!await LoadDiscussionAsync(threadId))
            return NotFound();

        return Page();
    }

    public async Task<IActionResult> OnPostReplyAsync(int threadId)
    {
        if (!await LoadDiscussionAsync(threadId))
            return NotFound();

        if (!ModelState.IsValid)
            return Page();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
            return Challenge();

        if (Input.ParentPostId.HasValue)
        {
            var existingPosts = await _posts.GetForThreadAsync(threadId);

            if (!existingPosts.Any(p => p.Id == Input.ParentPostId.Value))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The parent reply does not belong to this discussion.");

                return Page();
            }
        }

        var post = new Post
        {
            Title = "Reply",
            Content = Input.Content.Trim(),
            DiscussionThreadId = threadId,
            ParentPostId = Input.ParentPostId,
            ApplicationUserId = userId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _posts.AddAsync(post);

        return RedirectToPage(
            "/Discussions/Details",
            new { threadId });
    }

    private async Task<bool> LoadDiscussionAsync(int threadId)
    {
        Thread = await _threads.GetAsync(threadId);

        if (Thread == null)
            return false;

        var posts = await _posts.GetForThreadAsync(threadId);

        // Confirm this method name matches your supplied
        // PostTreeBuilder.cs before building.
        PostTree = PostTreeBuilder.Build(posts);

        return true;
    }
}
