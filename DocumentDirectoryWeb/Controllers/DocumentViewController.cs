using DocumentDirectoryWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace DocumentDirectoryWeb.Controllers;

public class DocumentViewController : Controller
{
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly ApplicationContext _context;

    public DocumentViewController(IWebHostEnvironment hostingEnvironment, ApplicationContext context)
    {
        _hostingEnvironment = hostingEnvironment;
        _context = context;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var random = new Random();

        var documents = _context.Documents.ToList().Select(e =>
        {
            var isReviewed = random.Next(2) != 0;
            return new DocumentView
            {
                Id = e.Id,
                Name = e.Name,
                IsReviewed = isReviewed,
                ReviewDate = isReviewed ? DateTime.Now.AddDays(random.Next(-30, 30)) : null
            };
        }).ToList();
        
        return View(documents);
    }
    
    [HttpGet]
    public IActionResult GetPdf(string fileId)
    {
        // Создаем путь до файла
        var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "files", "pdf", $"{fileId}.pdf");

        try
        {
            // Проверяем, существует ли файл
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

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