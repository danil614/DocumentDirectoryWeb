using System.ComponentModel.DataAnnotations;

namespace DocumentDirectoryWeb.Models;

public class UserType
{
    [Key] public int Id { get; set; }

    [Required(ErrorMessage = "Поле является обязательным.")]
    [Display(Name = "Название")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Поле является обязательным.")]
    [Display(Name = "Системное имя")]
    public string SystemName { get; set; } = null!;
}