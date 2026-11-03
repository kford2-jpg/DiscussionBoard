using DiscussionBoard.Data;
using Microsoft.EntityFrameworkCore;

namespace DiscussionBoard.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly ApplicationDbContext _context;

        public UserProfileService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsDisplayNameAvailableAsync(
            string displayName,
            string? excludeUserId = null)
        {
            if (string.IsNullOrWhiteSpace(displayName))
                return false;

            string trimmedName = displayName.Trim();

            if (trimmedName.Length < 2 || trimmedName.Length > 60)
                return false;

            var query = _context.Users
                .Where(user =>
                    user.DisplayName.ToLower() == trimmedName.ToLower());

            if (excludeUserId != null)
            {
                query = query.Where(user => user.Id != excludeUserId);
            }

            return !await query.AnyAsync();
        }
    }
}
