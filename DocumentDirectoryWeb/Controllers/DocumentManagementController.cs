using DocumentDirectoryWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Office.Interop.Word;
using Document = DocumentDirectoryWeb.Models.Document;

namespace DocumentDirectoryWeb.Controllers;

[Authorize(Roles = "Admin, Editor")]
public class DocumentManagementController : Controller
{
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly ApplicationContext _context;

    public DocumentManagementController(IWebHostEnvironment hostingEnvironment, ApplicationContext context)
    {
        _hostingEnvironment = hostingEnvironment;
        _context = context;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var documents = _context.Documents.Include(e => e.Category).ToList();
        return View(documents);
    }

    [HttpGet]
    public IActionResult CreateItem()
    {
        var item = new Document();

        ViewBag.Categories = _context.DocumentCategories.ToList();
        ViewBag.Edit = false;

        return PartialView("Form", item);
    }

    [HttpPost]
    public IActionResult SaveItem(string name, long categoryId, IFormFile file, bool isEdit)
    {
        // Генерируем GUID
        var guid = Guid.NewGuid().ToString();

        var isAdded = SaveFile(file, guid, out var errorMessage);
        if (!isAdded) return StatusCode(StatusCodes.Status500InternalServerError, new { errorMessage });

        // TODO: Проверка на корректность ввода

        var item = new Document
        {
            Id = guid,
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

        try
        {
            // Получаем расширение файла
            var extension = Path.GetExtension(file.FileName);
            string originalFilePath;
            message = null;
            var isPdf = extension is ".pdf";

            if (isPdf)
            {
                originalFilePath = Path.Combine(_hostingEnvironment.WebRootPath, "files", "pdf", $"{fileId}.pdf");
            }
            else if (extension is ".doc" or ".docx")
            {
                // Формируем имя файла с использованием GUID
                var fileName = fileId + extension;
                originalFilePath = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", fileName);
            }
            else
            {
                message = "Произошла ошибка: разрешены только файлы PDF, DOC и DOCX.";
                return false;
            }

            using (var stream = new FileStream(originalFilePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            // Если файл pdf, его не нужно преобразовывать
            if (isPdf) return true;

            // Иначе преобразовываем doc/docx в pdf
            var pdfFilePath = Path.Combine(_hostingEnvironment.WebRootPath, "files", "pdf", $"{fileId}.pdf");
            ConvertToPdf(originalFilePath, pdfFilePath);

            //TODO: System.IO.File.Delete(filePath);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            message = $"Произошла ошибка: {ex.Message}";
            return false;
        }
    }

    private static void ConvertToPdf(string originalFilePath, string pdfFilePath)
    {
        var wordApplication = new Application();
        var wordDocument = wordApplication.Documents.Open(originalFilePath);

        wordDocument.SaveAs(pdfFilePath, WdSaveFormat.wdFormatPDF);

        wordDocument.Close();
        wordApplication.Quit();
    }
}