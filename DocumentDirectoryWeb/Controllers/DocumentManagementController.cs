using DocumentDirectoryWeb.Helpers;
using DocumentDirectoryWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DocumentDirectoryWeb.Controllers;

[Authorize(Roles = "Admin, Editor")]
public class DocumentManagementController : Controller
{
    private readonly ApplicationContext _context;
    private readonly IWebHostEnvironment _hostingEnvironment;

    public DocumentManagementController(IWebHostEnvironment hostingEnvironment, ApplicationContext context)
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
            documents = _context.Documents.Include(e => e.Category).ToList().AsQueryable();
        }
        else
        {
            documents = _context.Documents.Include(e => e.Category)
                .Where(e => e.CategoryId == categoryId).ToList().AsQueryable();
            ViewBag.title = categoryName;
        }

        return View(documents);
    }

    [HttpGet]
    public IActionResult GetSortedData(string sortBy, string sortDirection)
    {
        var data = _context.Documents.Include(e => e.Category).ToList().AsQueryable();

        data = sortBy switch
        {
            "name" => sortDirection == "asc"
                ? data.OrderBy(item => item.Name)
                : data.OrderByDescending(item => item.Name),
            "category" => sortDirection == "asc"
                ? data.OrderBy(item => item.Category)
                : data.OrderByDescending(item => item.Category),
            _ => data
        };

        return PartialView("_Table", data);
    }

    [HttpPost]
    [Authorize(Roles = "Admin, Editor")]
    public IActionResult DeleteItem(string id)
    {
        var item = _context.Documents.FirstOrDefault(item => item.Id == id);
        if (item == null) return NotFound(); // Если запись не найдена, возвращаем ошибку 404

        // Удаляем файл
        var pdfFilePath = Path.Combine(_hostingEnvironment.WebRootPath, "files", "pdf", $"{item.Id}.pdf");
        System.IO.File.Delete(pdfFilePath);

        _context.Documents.Remove(item);

        var rowsAffected = _context.SaveChanges();
        return rowsAffected > 0 ? Ok() : StatusCode(StatusCodes.Status500InternalServerError);
    }

    [HttpGet]
    public IActionResult GetItem(string id)
    {
        var item = _context.Documents.FirstOrDefault(item => item.Id == id);
        if (item == null) return NotFound(); // Если запись не найдена, возвращаем ошибку 404

        ViewBag.Categories = _context.DocumentCategories.OrderBy(d => d.Name).ToList();
        ViewBag.Edit = true;

        return PartialView("Form", item);
    }

    [HttpGet]
    public IActionResult CreateItem()
    {
        var item = new Document();

        ViewBag.Categories = _context.DocumentCategories.OrderBy(d => d.Name).ToList();
        ViewBag.Edit = false;

        return PartialView("Form", item);
    }

    [HttpPost]
    public IActionResult SaveItem(string id, string name, int categoryId, IFormFile? file, bool isEdit)
    {
        if (string.IsNullOrEmpty(name)) return StatusCode(StatusCodes.Status500InternalServerError);

        if (!isEdit)
            // Генерируем GUID
            id = Guid.NewGuid().ToString();

        if (!isEdit || file != null)
        {
            var isAdded = SaveFile(file, id, out var errorMessage);
            if (!isAdded) return StatusCode(StatusCodes.Status500InternalServerError, new { errorMessage });
        }

        var item = new Document
        {
            Id = id,
            Name = name,
            CategoryId = categoryId
        };

        if (isEdit)
            _context.Documents.Update(item);
        else
            _context.Documents.Add(item);

        var rowsAffected = _context.SaveChanges();
        return rowsAffected > 0 ? Ok() : StatusCode(StatusCodes.Status500InternalServerError);
    }

    private bool SaveFile(IFormFile? file, string fileId, out string? message)
    {
        if (file == null || file.Length <= 0)
        {
            message = "Файл не найден.";
            return false;
        }
        
        // Получаем расширение файла
        var extension = Path.GetExtension(file.FileName);
        string originalFilePath;
        message = null;
        var isPdf = extension is ".pdf";

        if (isPdf)
        {
            originalFilePath = Path.Combine(_hostingEnvironment.WebRootPath, "files", "pdf", $"{fileId}.pdf");
        }
        else if (extension is ".docx")
        {
            // Формируем имя файла с использованием GUID
            var fileName = fileId + extension;
            originalFilePath = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", fileName);
        }
        else
        {
            message = "Произошла ошибка: разрешены только файлы PDF и DOCX.";
            return false;
        }

        try
        {
            using (var stream = new FileStream(originalFilePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            // Если файл pdf, его не нужно преобразовывать
            if (isPdf) return true;

            // Иначе преобразовываем docx в pdf
            var pdfFilePath = Path.Combine(_hostingEnvironment.WebRootPath, "files", "pdf", $"{fileId}.pdf");
            Converter.DocxToPdf(originalFilePath, pdfFilePath);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            message = $"Произошла ошибка: {ex.Message}";
            return false;
        }
        finally
        {
            if (!isPdf)
            {
                System.IO.File.Delete(originalFilePath);
            }
        }
    }
}