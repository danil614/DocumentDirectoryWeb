using Microsoft.EntityFrameworkCore;

namespace DocumentDirectoryWeb.Models;

public class ApplicationContext : DbContext
{
    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
    {
        // ReSharper disable once VirtualMemberCallInConstructor
        Database.EnsureCreated();
    }
    
    public DbSet<Document> Documents { get; set; } = null!;
    public DbSet<DocumentCategory> DocumentCategories { get; set; } = null!;
    
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
    }
}