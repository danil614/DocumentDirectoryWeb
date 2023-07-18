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
    public IActionResult Index(int? categoryId, string? categoryName)
    {
        IQueryable<Document> documents;

        if (categoryId is null || categoryName is null)
        {
            documents = _context.Documents.Include(e => e.Category);
            ViewBag.showCategory = true;
        }
        else
        {
            documents = _context.Documents.Include(e => e.Category)
                .Where(d => d.CategoryId == categoryId);
            ViewBag.title = categoryName;
        }

        var userId = UserTabManager.GetUserId(User.Claims);

        var viewDocuments = documents.ToList().Select(e =>
        {
            var userDocumentReview =
                _context.UserDocumentReviews.FirstOrDefault(r => r.DocumentId == e.Id && r.UserId == userId);
            var isReviewed = false;
            DateTime? reviewDate = null;

            if (userDocumentReview is not null)
            {
                isReviewed = userDocumentReview.IsReviewed;
                reviewDate = userDocumentReview.ReviewDate;
            }

            return new DocumentView
            {
                DocumentId = e.Id,
                Name = e.Name,
                IsReviewed = isReviewed,
                ReviewDate = reviewDate,
                CategoryId = e.CategoryId,
                Category = e.Category
            };
        }).ToList();

        return View(viewDocuments);
    }

    [HttpPost]
    public IActionResult SaveReview(string documentId, bool isReviewed)
    {
        var userId = UserTabManager.GetUserId(User.Claims);
        if (userId is null) return StatusCode(StatusCodes.Status500InternalServerError);

        var userDocumentReview =
            _context.UserDocumentReviews.FirstOrDefault(r => r.DocumentId == documentId && r.UserId == userId);

        if (userDocumentReview is null)
        {
            userDocumentReview = new UserDocumentReview
            {
                UserId = userId,
                DocumentId = documentId,
                IsReviewed = isReviewed,
                ReviewDate = isReviewed ? DateTime.Now : null
            };
            _context.UserDocumentReviews.Add(userDocumentReview);
        }
        else
        {
            userDocumentReview.IsReviewed = isReviewed;
            userDocumentReview.ReviewDate = isReviewed ? DateTime.Now : null;
            _context.UserDocumentReviews.Update(userDocumentReview);
        }

        var rowsAffected = _context.SaveChanges();
        return rowsAffected > 0 ? Ok() : StatusCode(StatusCodes.Status500InternalServerError);
    }

    [HttpGet]
    public IActionResult GetCategories()
    {
        var categories = _context.DocumentCategories.OrderBy(d => d.Name).ToList().AsQueryable();
        return PartialView("_Categories", categories);
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