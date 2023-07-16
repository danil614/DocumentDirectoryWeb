using System.ComponentModel.DataAnnotations;

namespace DocumentDirectoryWeb.Models;

public class Document
{
    [Key] public string Id { get; set; } = null!;

    [Required(ErrorMessage = "Поле является обязательным.")]
    [Display(Name = "Название")]
    public string Name { get; set; } = null!;

    [Display(Name = "Раздел")]
    [Required(ErrorMessage = "Поле является обязательным.")]
    public int CategoryId { get; set; }

    [Display(Name = "Раздел")] public virtual DocumentCategory? Category { get; set; }
}