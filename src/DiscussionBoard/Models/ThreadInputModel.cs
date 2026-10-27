using System.ComponentModel.DataAnnotations;

namespace DiscussionBoard.Models;

public class ThreadInputModel
{
    [Required(ErrorMessage = "Enter a title.")]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter your discussion content.")]
    [StringLength(10000)]
    public string Content { get; set; } = string.Empty;
}
