namespace DocumentDirectoryWeb.Models;

public class DocumentView
{
    public string DocumentId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool IsReviewed { get; set; }
    public DateTime? ReviewDate { get; set; }
    public int CategoryId { get; set; }
    public virtual DocumentCategory? Category { get; set; }
}