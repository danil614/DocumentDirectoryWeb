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
        IQueryable<Document>? documents;

        if (categoryId is null || categoryName is null)
        {
            documents = _context.Documents.Include(d => d.Categories)
                .OrderBy(d => d.Name).ToList().AsQueryable();
            ViewBag.showCategory = true;
        }
        else
        {
            documents = _context.Categories.Include(c => c.Documents)
                .FirstOrDefault(c => c.Id == categoryId)?.Documents?.OrderBy(d => d.Name)
                .ToList().AsQueryable();
            ViewBag.title = categoryName;
        }

        return View(documents);
    }

    [HttpGet]
    public IActionResult GetSortedData(string sortBy, string sortDirection)
    {
        var data = _context.Documents.Include(d => d.Categories).ToList().AsQueryable();

        data = sortBy switch
        {
            "name" => sortDirection == "asc"
                ? data.OrderBy(item => item.Name)
                : data.OrderByDescending(item => item.Name),
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
        var item = _context.Documents.Include(d => d.Categories)
            .FirstOrDefault(item => item.Id == id);
        if (item == null) return NotFound(); // Если запись не найдена, возвращаем ошибку 404

        ViewBag.Categories = _context.Categories.OrderBy(d => d.Name).ToList();
        ViewBag.Edit = true;

        return PartialView("Form", item);
    }

    [HttpGet]
    public IActionResult CreateItem()
    {
        var item = new Document();

        ViewBag.Categories = _context.Categories.OrderBy(d => d.Name).ToList();
        ViewBag.Edit = false;

        return PartialView("Form", item);
    }

    [HttpPost]
    public IActionResult SaveItem(string id, string name, List<int> selectedCategoryIds, IFormFile? file, bool isEdit)
    {
        if (string.IsNullOrEmpty(name)) return StatusCode(StatusCodes.Status500InternalServerError);

        if (!isEdit) id = Guid.NewGuid().ToString(); // Генерируем GUID

        if (!isEdit || file != null)
        {
            var isAdded = SaveFile(file, id, out var errorMessage);
            if (!isAdded) return StatusCode(StatusCodes.Status500InternalServerError, new { errorMessage });
        }

        var selectedCategories = selectedCategoryIds
            .Select(selectedCategory => _context.Categories.FirstOrDefault(c => c.Id == selectedCategory))
            .ToList();

        var document = _context.Documents.Include(d => d.Categories)
            .FirstOrDefault(d => d.Id == id);

        if (document is null)
        {
            document = new Document
            {
                Id = id,
                Name = name,
                Categories = selectedCategories!
            };
            _context.Documents.Add(document);
        }
        else
        {
            document.Name = name;
            UpdateCategoryDocument(selectedCategories, ref document);
            _context.Documents.Update(document);
        }

        var rowsAffected = _context.SaveChanges();
        return rowsAffected > 0 ? Ok() : StatusCode(StatusCodes.Status500InternalServerError);
    }

    private static void UpdateCategoryDocument(List<Category?> selectedCategories, ref Document document)
    {
        var existingCategories = document.Categories?.ToList() ?? new List<Category>();

        var onDeleteCategories = existingCategories.Except(selectedCategories).ToList();
        var onAddCategories = selectedCategories.Except(existingCategories).ToList();

        foreach (var category in onDeleteCategories)
            if (category != null)
                document.Categories?.Remove(category);

        foreach (var category in onAddCategories)
            if (category != null)
                document.Categories?.Add(category);
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
            if (!isPdf) System.IO.File.Delete(originalFilePath);
        }
    }
}