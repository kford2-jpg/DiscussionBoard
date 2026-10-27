using System.ComponentModel.DataAnnotations;

namespace DiscussionBoard.Models
{
    public abstract class DiscussionBase : EntityBase
    {
        [Required]
        [StringLength(200)]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(10000)]
        [MaxLength(10000)]
        public string Content { get; set; } = string.Empty;

        public DateTimeOffset CreatedAt { get; set; }

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        public ApplicationUser ApplicationUser { get; set; } = null!;
    }
}
