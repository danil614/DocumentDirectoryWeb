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
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<UserType> UserTypes { get; set; } = null!;
    public DbSet<Department> Departments { get; set; } = null!;
    public DbSet<UserDocumentReview> UserDocumentReviews { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Госслужащие" },
            new Category { Id = 2, Name = "Работники" },
            new Category { Id = 3, Name = "Кадровый резерв" },
            new Category { Id = 4, Name = "Антикоррупция" }
        );

        modelBuilder.Entity<Document>().HasData(
            new Document { Id = "1", Name = "Документ Тест Перепись 1" },
            new Document { Id = "2", Name = "Документ Тест Проектная документация" },
            new Document { Id = "3", Name = "Документ Тест Заработная Плата" }
        );

        modelBuilder.Entity<Department>().HasData(
            new Department { Id = 1, Name = "Административный отдел" },
            new Department { Id = 2, Name = "Рыночных услуг" },
            new Department { Id = 3, Name = "Цен и финансов" },
            new Department { Id = 4, Name = "Региональных счетов и балансов" }
        );

        modelBuilder.Entity<UserType>().HasData(
            new UserType { Id = 1, Name = "Обычный пользователь", SystemName = "User" },
            new UserType { Id = 2, Name = "Редактор", SystemName = "Editor" },
            new UserType { Id = 3, Name = "Администратор", SystemName = "Admin" }
        );
    }
}