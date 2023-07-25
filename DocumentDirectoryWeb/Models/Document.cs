using System.ComponentModel.DataAnnotations;

namespace DocumentDirectoryWeb.Models;

public class Document
{
    [Key] public string Id { get; set; } = null!;

    [Required(ErrorMessage = "Поле является обязательным.")]
    [Display(Name = "Название")]
    public string Name { get; set; } = null!;

    [Display(Name = "Разрешать ознакомление")]
    public bool AllowReview { get; set; }

    public virtual List<Category>? Categories { get; set; }

    public virtual List<UserDocumentReview>? UserDocumentReviews { get; set; }

    public string GetCategories()
    {
        var stringCategories = "";

        if (Categories != null)
            foreach (var category in Categories)
                stringCategories += category.Name + (category == Categories.Last() ? "" : "; ");

        return stringCategories;
    }
}