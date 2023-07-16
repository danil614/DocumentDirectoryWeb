using System.ComponentModel.DataAnnotations;

namespace DocumentDirectoryWeb.Models;

public class DocumentCategory
{
    [Key] public long Id { get; set; }

    [Required(ErrorMessage = "Поле является обязательным.")]
    [Display(Name = "Название")]
    public string Name { get; set; } = null!;

    public override string ToString()
    {
        return Name;
    }
}