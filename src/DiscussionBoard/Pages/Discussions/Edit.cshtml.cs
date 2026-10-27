using System.Security.Claims;
using DiscussionBoard.Models;
using DiscussionBoard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DiscussionBoard.Pages.Discussions;

[Authorize]
public class EditModel : PageModel
{
    private readonly DiscussionThreadService _threads;

    [BindProperty]
    public int Id { get; set; }

    [BindProperty]
    public ThreadInputModel Input { get; set; } = new();

    public EditModel(DiscussionThreadService threads)
    {
        _threads = threads;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var thread = await _threads.GetAsync(id);

        if (thread == null)
            return NotFound();

        if (!IsAuthor(thread))
            return Forbid();

        Id = thread.Id;
        Input.Title = thread.Title;
        Input.Content = thread.Content;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var thread = await _threads.GetAsync(Id);

        if (thread == null)
            return NotFound();

        if (!IsAuthor(thread))
            return Forbid();

        if (!ModelState.IsValid)
            return Page();

        // Only change the title and content.
        thread.Title = Input.Title.Trim();
        thread.Content = Input.Content.Trim();

        var updated = await _threads.UpdateAsync(thread);

        if (!updated)
            return NotFound();

        return RedirectToPage(
            "/Discussions/Details",
            new { threadId = thread.Id });
    }

    private bool IsAuthor(DiscussionThread thread)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return userId != null &&
               userId == thread.ApplicationUserId;
    }
}
