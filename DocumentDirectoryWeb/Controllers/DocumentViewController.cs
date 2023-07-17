using DocumentDirectoryWeb.Helpers;
using DocumentDirectoryWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DocumentDirectoryWeb.Controllers;

[Authorize(Roles = "Admin, Editor, User")]
public class DocumentViewController : Controller
{
    private readonly ApplicationContext _context;
    private readonly IWebHostEnvironment _hostingEnvironment;

    public DocumentViewController(IWebHostEnvironment hostingEnvironment, ApplicationContext context)
    {
        _hostingEnvironment = hostingEnvironment;
        _context = context;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var random = new Random();

        var documents = _context.Documents.Include(e => e.Category).ToList()
            .Select(e =>
            {
                var isReviewed = random.Next(2) != 0;
                return new DocumentView
                {
                    DocumentId = e.Id,
                    Name = e.Name,
                    IsReviewed = isReviewed,
                    ReviewDate = isReviewed ? DateTime.Now.AddDays(random.Next(-30, 30)) : null,
                    CategoryId = e.CategoryId,
                    Category = e.Category
                };
            }).ToList();

        ViewBag.showCategory = true;
        return View(documents);
    }

    [HttpGet]
    public IActionResult GetDocumentsByCategory(int categoryId, string categoryName)
    {
        var random = new Random();

        var documents = _context.Documents.Include(e => e.Category)
            .Where(d => d.CategoryId == categoryId).ToList()
            .Select(e =>
            {
                var isReviewed = random.Next(2) != 0;
                return new DocumentView
                {
                    DocumentId = e.Id,
                    Name = e.Name,
                    IsReviewed = isReviewed,
                    ReviewDate = isReviewed ? DateTime.Now.AddDays(random.Next(-30, 30)) : null,
                    CategoryId = e.CategoryId,
                    Category = e.Category
                };
            }).ToList();

        ViewBag.title = categoryName;
        return View("Index", documents);
    }

    [HttpGet]
    [Route("GetDocument/{id}/{name}")]
    public IActionResult GetDocument(string id, string name)
    {
        // Создаем путь до файла
        var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "files", "pdf", $"{id}.pdf");

        try
        {
            // Проверяем, существует ли файл
            if (!System.IO.File.Exists(filePath)) return NotFound();

            // Открываем поток для чтения файла
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            // Отдаем файл клиенту
            return new FileStreamResult(fileStream, "application/pdf");
        }
        catch (Exception ex)
        {
            // Обработка ошибок, если возникла ошибка при чтении файла
            return Content($"Ошибка при открытии PDF: {ex.Message}");
        }
    }
}