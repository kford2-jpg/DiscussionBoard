
using System.Security.Claims;
using DiscussionBoard.Models;
using DiscussionBoard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DiscussionBoard.Pages.Discussions;

[Authorize]
public class CreateModel : PageModel
{
    private readonly ForumService _forums;
    private readonly DiscussionThreadService _threads;
    private readonly ILogger<CreateModel> _logger;

    public Forum? Forum { get; private set; }

    [BindProperty(SupportsGet = true)]
    public int ForumId { get; set; }

    [BindProperty]
    public ThreadInputModel Input { get; set; } = new();

    public CreateModel(
        ForumService forums,
        DiscussionThreadService threads,
        ILogger<CreateModel> logger)
    {
        _forums = forums;
        _threads = threads;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadForumAsync();
        return Forum == null ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadForumAsync();

        if (Forum == null)
            return NotFound();

        if (!ModelState.IsValid)
            return Page();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
            return Challenge();

        var thread = new DiscussionThread
        {
            Title = Input.Title.Trim(),
            Content = Input.Content.Trim(),
            ForumId = Forum.Id,
            ApplicationUserId = userId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        try
        {
            await _threads.AddAsync(thread);

            return RedirectToPage(
                "/Discussions/Details",
                new { threadId = thread.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create a discussion thread.");
            throw;
        }
    }

    private async Task LoadForumAsync()
    {
        var forums = await _forums.GetAllAsync();
        Forum = forums.FirstOrDefault(f => f.Id == ForumId);
    }
}
