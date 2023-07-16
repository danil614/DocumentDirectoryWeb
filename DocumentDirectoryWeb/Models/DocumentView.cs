namespace DocumentDirectoryWeb.Models;

public class DocumentView
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool IsReviewed { get; set; }
    public DateTime? ReviewDate { get; set; }
}