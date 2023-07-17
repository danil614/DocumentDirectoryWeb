using System.ComponentModel.DataAnnotations;

namespace DocumentDirectoryWeb.Models;

public class UserDocumentReview
{
    [Key] public int Id { get; set; }
    public string UserId { get; set; } = null!;
    public virtual User? User { get; set; }
    public string DocumentId { get; set; } = null!;
    public virtual Document? Document { get; set; }
    public bool IsReviewed { get; set; }
    public DateTime? ReviewDate { get; set; }
}