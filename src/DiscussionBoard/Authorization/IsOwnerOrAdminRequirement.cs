using System.Security.Claims;
using DiscussionBoard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace DiscussionBoard.Authorization;

public class ApplicationAuthorizationService
    : IApplicationAuthorizationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IAuthorizationService _authorizationService;

    public ApplicationAuthorizationService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IAuthorizationService authorizationService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _authorizationService = authorizationService;
    }

    public async Task<bool> CanEditAsync(
        ClaimsPrincipal principal,
        DiscussionBase resource)
    {
        var result = await _authorizationService.AuthorizeAsync(
            principal,
            resource,
            ApplicationAuthorizationPolicies.IsOwnerOrAdmin);

        return result.Succeeded;
    }

    public async Task<IdentityResult> AddRoleAsync(
        ClaimsPrincipal actingUser,
        string targetUserId,
        string roleName)
    {
        if (!IsAuthenticatedAdmin(actingUser))
        {
            return Failed(
                "Only authenticated administrators can change roles.");
        }

        if (string.IsNullOrWhiteSpace(targetUserId) ||
            string.IsNullOrWhiteSpace(roleName))
        {
            return Failed("A valid user and role are required.");
        }

        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            return Failed("The requested role does not exist.");
        }

        var targetUser =
            await _userManager.FindByIdAsync(targetUserId);

        if (targetUser == null)
        {
            return Failed("The target user does not exist.");
        }

        if (await _userManager.IsInRoleAsync(targetUser, roleName))
        {
            return Failed("The user already has this role.");
        }

        return await _userManager.AddToRoleAsync(
            targetUser,
            roleName);
    }

    public async Task<IdentityResult> RemoveRoleAsync(
        ClaimsPrincipal actingUser,
        string targetUserId,
        string roleName)
    {
        if (!IsAuthenticatedAdmin(actingUser))
        {
            return Failed(
                "Only authenticated administrators can change roles.");
        }

        if (string.IsNullOrWhiteSpace(targetUserId) ||
            string.IsNullOrWhiteSpace(roleName))
        {
            return Failed("A valid user and role are required.");
        }

        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            return Failed("The requested role does not exist.");
        }

        var targetUser =
            await _userManager.FindByIdAsync(targetUserId);

        if (targetUser == null)
        {
            return Failed("The target user does not exist.");
        }

        if (roleName == ApplicationRoles.Admin)
        {
            string? actingUserId =
                actingUser.FindFirstValue(ClaimTypes.NameIdentifier);

            if (targetUser.Id == actingUserId)
            {
                return Failed(
                    "Administrators cannot remove their own Admin role.");
            }

            var administrators =
                await _userManager.GetUsersInRoleAsync(
                    ApplicationRoles.Admin);

            if (administrators.Count <= 1)
            {
                return Failed(
                    "The last administrator cannot be removed.");
            }
        }

        return await _userManager.RemoveFromRoleAsync(
            targetUser,
            roleName);
    }

    private static bool IsAuthenticatedAdmin(
        ClaimsPrincipal principal)
    {
        return principal.Identity?.IsAuthenticated == true &&
               principal.IsInRole(ApplicationRoles.Admin);
    }

    private static IdentityResult Failed(string description)
    {
        return IdentityResult.Failed(
            new IdentityError
            {
                Description = description
            });
    }
}
