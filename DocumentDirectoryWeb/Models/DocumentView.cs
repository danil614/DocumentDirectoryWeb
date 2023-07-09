namespace DocumentDirectoryWeb.Models;

public class DocumentView
{
    public string Id { get; set; }
    public string Name { get; set; }
    public bool IsReviewed { get; set; }
    public DateTime? ReviewDate { get; set; }
}
