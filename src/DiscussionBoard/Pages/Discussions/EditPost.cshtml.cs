using DiscussionBoard.Authorization;
using DiscussionBoard.Models;
using DiscussionBoard.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DiscussionBoard.Pages;

[Authorize]
public class EditPostModel : PageModel
{
    private readonly PostService _posts;
    private readonly IApplicationAuthorizationService _authorization;
    private readonly ILogger<EditPostModel> _logger;

    [BindProperty]
    public DiscussionEditViewModel Input { get; set; } = new();

    public int ThreadId { get; set; }

    public int PostId { get; set; }

    public EditPostModel(
        PostService posts,
        IApplicationAuthorizationService authorization,
        ILogger<EditPostModel> logger)
    {
        _posts = posts;
        _authorization = authorization;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(
        int threadId,
        int postId)
    {
        var post = await _posts.GetAsync(postId);

        if (post == null)
            return NotFound();

        if (post.DiscussionThreadId != threadId)
            return NotFound();

        if (!await _authorization.CanEditAsync(User, post))
        {
            _logger.LogWarning(
                "Rejected edit attempt for post {PostId} in discussion thread {ThreadId}",
                post.Id,
                post.DiscussionThreadId);
            return Forbid();
        }

        ThreadId = threadId;
        PostId = postId;

        Input = new DiscussionEditViewModel
        {
            Title = post.Title,
            Content = post.Content
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(
        int threadId,
        int postId)
    {
        var post = await _posts.GetAsync(postId);

        if (post == null)
            return NotFound();

        if (post.DiscussionThreadId != threadId)
            return NotFound();

        if (!await _authorization.CanEditAsync(User, post))
        {
            _logger.LogWarning(
                "Rejected edit attempt for post {PostId} in discussion thread {ThreadId}",
                post.Id,
                post.DiscussionThreadId);
            return Forbid();
        }

        ThreadId = threadId;
        PostId = postId;

        if (!ModelState.IsValid)
            return Page();

        post.Title = Input.Title.Trim();
        post.Content = Input.Content.Trim();

        await _posts.UpdateAsync(post);

        return RedirectToPage(
            "/Discussions/Details",
            new { threadId });
    }
}
