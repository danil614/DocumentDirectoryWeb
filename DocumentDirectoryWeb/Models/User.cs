using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DocumentDirectoryWeb.Models;

[PrimaryKey(nameof(Id), nameof(Login))]
[Table("Users")]
public class User
{
    [Key]
    public string Id { get; set; } = null!;
    
    [Key]
    [Required(ErrorMessage = "Поле является обязательным.")]
    [Display(Name = "Имя пользователя")]
    public string Login { get; set; } = null!;

    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string? Password { get; set; }
    
    [Display(Name = "Тип пользователя")]
    public virtual UserType? UserType { get; set; }

    [Required(ErrorMessage = "Поле является обязательным.")]
    [Display(Name = "Тип пользователя")]
    public int UserTypeId { get; set; }
    
    [Required(ErrorMessage = "Поле является обязательным.")]
    [Display(Name = "ФИО")]
    public string FullName { get; set; } = null!;
    
    [Display(Name = "Подразделение")]
    public virtual Department? Department { get; set; }

    [Required(ErrorMessage = "Поле является обязательным.")]
    [Display(Name = "Подразделение")]
    public int DepartmentId { get; set; }
}