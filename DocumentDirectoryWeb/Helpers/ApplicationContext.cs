using DocumentDirectoryWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace DocumentDirectoryWeb.Helpers;

public class ApplicationContext : DbContext
{
    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
    {
        // ReSharper disable once VirtualMemberCallInConstructor
        Database.EnsureCreated();
    }

    public DbSet<Document> Documents { get; set; } = null!;
    public DbSet<DocumentCategory> DocumentCategories { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<UserType> UserTypes { get; set; } = null!;
    public DbSet<Department> Departments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DocumentCategory>().HasData(
            new DocumentCategory { Id = 1, Name = "Tom" },
            new DocumentCategory { Id = 2, Name = "Bob" },
            new DocumentCategory { Id = 3, Name = "Sam" }
        );

        modelBuilder.Entity<Document>().HasData(
            new Document { Id = "1", Name = "Документ Тест Перепись 1", CategoryId = 1 },
            new Document { Id = "2", Name = "Документ Тест Проектная документация", CategoryId = 2 },
            new Document { Id = "3", Name = "Документ Тест Заработная Плата", CategoryId = 2 }
        );

        modelBuilder.Entity<Department>().HasData(
            new Department { Id = 1, Name = "Административный отдел" },
            new Department { Id = 2, Name = "Отдел статистики рыночных услуг" },
            new Department { Id = 3, Name = "Отдел статистики цен и финансов" }
        );

        modelBuilder.Entity<UserType>().HasData(
            new UserType { Id = 1, Name = "Обычный пользователь", SystemName = "User" },
            new UserType { Id = 2, Name = "Редактор", SystemName = "Editor" },
            new UserType { Id = 3, Name = "Администратор", SystemName = "Admin" }
        );
    }
}