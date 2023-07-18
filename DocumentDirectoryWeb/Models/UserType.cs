using System.ComponentModel.DataAnnotations;

namespace DocumentDirectoryWeb.Models;

public class UserType : IComparable<UserType>
{
    [Key] public int Id { get; set; }

    [Required(ErrorMessage = "Поле является обязательным.")]
    [Display(Name = "Название")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Поле является обязательным.")]
    [Display(Name = "Системное имя")]
    public string SystemName { get; set; } = null!;

    public int CompareTo(UserType? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        return string.Compare(Name, other.Name, StringComparison.Ordinal);
    }

    public override string ToString()
    {
        return Name;
    }
}