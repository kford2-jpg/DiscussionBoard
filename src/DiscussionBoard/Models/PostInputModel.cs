using System.ComponentModel.DataAnnotations;

namespace DiscussionBoard.Models;

public class PostInputModel
{
    [Required(ErrorMessage = "Enter a reply.")]
    [StringLength(10000)]
    public string Content { get; set; } = string.Empty;

    public int? ParentPostId { get; set; }
}
