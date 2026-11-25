using DiscussionBoard.Authorization;
using DiscussionBoard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DiscussionBoard.Pages.Admin;

[Authorize(Roles = ApplicationRoles.Admin)]
public class UsersModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IApplicationAuthorizationService _authorizationService;

    public UsersModel(
        UserManager<ApplicationUser> userManager,
        IApplicationAuthorizationService authorizationService)
    {
        _userManager = userManager;
        _authorizationService = authorizationService;
    }

    public IList<ApplicationUser> Users { get; private set; } = new List<ApplicationUser>();
    public ApplicationUser? SelectedUser { get; private set; }
    public IList<string> SelectedUserRoles { get; private set; } = new List<string>();

    [BindProperty(SupportsGet = true)]
    public string? UserId { get; set; }

    [BindProperty]
    public string? SelectedUserId { get; set; }

    [BindProperty]
    public string? SelectedRole { get; set; }

    [BindProperty]
    public string? RoleAction { get; set; }

    public async Task OnGetAsync() => await LoadPageDataAsync();

    public async Task<IActionResult> OnPostUpdateRoleAsync()
    {
        UserId = string.IsNullOrWhiteSpace(SelectedUserId) ? UserId : SelectedUserId;
        return await ChangeAdminRoleAsync(redirectOnFailure: false);
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        UserId = SelectedUserId;
        SelectedRole = ApplicationRoles.Admin;
        RoleAction = "Add";
        return await ChangeAdminRoleAsync(redirectOnFailure: true);
    }

    public async Task<IActionResult> OnPostRemoveAsync()
    {
        UserId = SelectedUserId;
        SelectedRole = ApplicationRoles.Admin;
        RoleAction = "Remove";
        return await ChangeAdminRoleAsync(redirectOnFailure: true);
    }

    private async Task<IActionResult> ChangeAdminRoleAsync(bool redirectOnFailure)
    {
        if (string.IsNullOrWhiteSpace(UserId))
        {
            ModelState.AddModelError(string.Empty, "Select a user.");
            return await InvalidResultAsync(redirectOnFailure);
        }

        if (!string.Equals(SelectedRole, ApplicationRoles.Admin, StringComparison.Ordinal))
        {
            ModelState.AddModelError(nameof(SelectedRole), "Only the Admin role can be managed.");
            return await InvalidResultAsync(redirectOnFailure);
        }

        IdentityResult result;
        if (RoleAction == "Add")
        {
            result = await _authorizationService.AddRoleAsync(User, UserId, ApplicationRoles.Admin);
        }
        else if (RoleAction == "Remove")
        {
            result = await _authorizationService.RemoveRoleAsync(User, UserId, ApplicationRoles.Admin);
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Select a valid role action.");
            return await InvalidResultAsync(redirectOnFailure);
        }

        if (result.Succeeded)
        {
            TempData["StatusMessage"] =
                $"The Admin role was successfully {(RoleAction == "Add" ? "added" : "removed")}.";
            TempData["StatusIsError"] = false;
            return RedirectToPage(new { userId = UserId });
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        if (redirectOnFailure)
        {
            TempData["StatusMessage"] = string.Join(" ", result.Errors.Select(error => error.Description));
            TempData["StatusIsError"] = true;
            return RedirectToPage(new { userId = UserId });
        }

        await LoadPageDataAsync();
        return Page();
    }

    private async Task<IActionResult> InvalidResultAsync(bool redirectOnFailure)
    {
        if (redirectOnFailure)
        {
            TempData["StatusMessage"] = string.Join(" ", ModelState.Values
                .SelectMany(value => value.Errors)
                .Select(error => error.ErrorMessage));
            TempData["StatusIsError"] = true;
            return RedirectToPage(new { userId = UserId });
        }

        await LoadPageDataAsync();
        return Page();
    }

    private async Task LoadPageDataAsync()
    {
        Users = await _userManager.Users
            .OrderBy(user => user.DisplayName)
            .ThenBy(user => user.Email)
            .ToListAsync();

        if (string.IsNullOrWhiteSpace(UserId))
        {
            SelectedUser = null;
            SelectedUserRoles = new List<string>();
            SelectedUserId = null;
            return;
        }

        SelectedUser = await _userManager.FindByIdAsync(UserId);
        if (SelectedUser == null)
        {
            ModelState.AddModelError(string.Empty, "The selected user could not be found.");
            SelectedUserRoles = new List<string>();
            return;
        }

        SelectedUserId = SelectedUser.Id;
        SelectedUserRoles = (await _userManager.GetRolesAsync(SelectedUser)).ToList();
    }
}