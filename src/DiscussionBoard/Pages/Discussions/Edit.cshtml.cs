using DiscussionBoard.Authorization;
using DiscussionBoard.Models;
using DiscussionBoard.Services;
using DiscussionBoard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DiscussionBoard.Pages.Discussions;

[Authorize]
public class ThreadEditModel : PageModel
{
    private readonly DiscussionThreadService _threads;
    private readonly IAuthorizationService _authorization;

    [BindProperty]
    public int Id { get; set; }

    [BindProperty]
    public DiscussionEditViewModel Input { get; set; } = new();

    public EditModel(
        DiscussionThreadService threads,
        IAuthorizationService authorization)
    {
        _threads = threads;
        _authorization = authorization;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var thread = await _threads.GetAsync(id);

        if (thread == null)
            return NotFound();

        var result = await _authorization.AuthorizeAsync(
            User,
            thread,
            ApplicationAuthorizationPolicies.IsOwnerOrAdmin);

        if (!result.Succeeded)
            return Forbid();

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

        var result = await _authorization.AuthorizeAsync(
            User,
            thread,
            ApplicationAuthorizationPolicies.IsOwnerOrAdmin);

        if (!result.Succeeded)
            return Forbid();

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
