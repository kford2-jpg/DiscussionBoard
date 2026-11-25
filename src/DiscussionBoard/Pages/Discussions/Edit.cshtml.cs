using DiscussionBoard.Authorization;
using DiscussionBoard.Models;
using DiscussionBoard.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DiscussionBoard.Pages;

[Authorize]
public class ThreadEditModel : PageModel
{
    private readonly DiscussionThreadService _threads;
    private readonly IApplicationAuthorizationService _authorization;
    private readonly ILogger<ThreadEditModel> _logger;

    [BindProperty]
    public int Id { get; set; }

    [BindProperty]
    public DiscussionEditViewModel Input { get; set; } = new();

    public ThreadEditModel(
        DiscussionThreadService threads,
        IApplicationAuthorizationService authorization,
        ILogger<ThreadEditModel> logger)
    {
        _threads = threads;
        _authorization = authorization;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var thread = await _threads.GetAsync(id);

        if (thread == null)
            return NotFound();

        if (!await _authorization.CanEditAsync(User, thread))
        {
            _logger.LogWarning(
                "Rejected edit attempt for discussion thread {ThreadId}",
                thread.Id);
            return Forbid();
        }

        Id = thread.Id;

        Input = new DiscussionEditViewModel
        {
            Title = thread.Title,
            Content = thread.Content
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var thread = await _threads.GetAsync(Id);

        if (thread == null)
            return NotFound();

        if (!await _authorization.CanEditAsync(User, thread))
        {
            _logger.LogWarning(
                "Rejected edit attempt for discussion thread {ThreadId}",
                thread.Id);
            return Forbid();
        }

        if (!ModelState.IsValid)
            return Page();

        thread.Title = Input.Title.Trim();
        thread.Content = Input.Content.Trim();

        var updated = await _threads.UpdateAsync(thread);

        if (!updated)
            return NotFound();

        return RedirectToPage(
            "/Discussions/Details",
            new { threadId = thread.Id });
    }
}
