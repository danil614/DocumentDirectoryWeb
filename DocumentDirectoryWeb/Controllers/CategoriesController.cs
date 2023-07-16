using DocumentDirectoryWeb.Helpers;
using DocumentDirectoryWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocumentDirectoryWeb.Controllers;

[Authorize(Roles = "Admin, Editor")]
public class CategoriesController : Controller
{
    private readonly ApplicationContext _context;

    public CategoriesController(ApplicationContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(_context.DocumentCategories.OrderBy(item => item.Name).ToList().AsQueryable());
    }

    [HttpGet]
    public IActionResult GetSortedData(string sortBy, string sortDirection)
    {
        var data = _context.DocumentCategories.ToList().AsQueryable();

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
    [Authorize(Roles = "Admin")]
    public IActionResult DeleteItem(int id)
    {
        var item = _context.DocumentCategories.FirstOrDefault(item => item.Id == id);
        if (item == null) return NotFound(); // Если запись не найдена, возвращаем ошибку 404

        _context.DocumentCategories.Remove(item);
        
        var rowsAffected = _context.SaveChanges();
        return rowsAffected > 0 ? Ok() : StatusCode(StatusCodes.Status500InternalServerError);
    }

    [HttpGet]
    public IActionResult GetItem(int id)
    {
        var item = _context.DocumentCategories.FirstOrDefault(item => item.Id == id);
        if (item == null) return NotFound(); // Если запись не найдена, возвращаем ошибку 404

        ViewBag.Edit = true;

        return PartialView("Form", item);
    }

    [HttpGet]
    public IActionResult CreateItem()
    {
        var item = new DocumentCategory();
        ViewBag.Edit = false;

        return PartialView("Form", item);
    }

    [HttpPost]
    public IActionResult SaveItem(DocumentCategory item)
    {
        if (ModelState.IsValid)
        {
            _context.DocumentCategories.Update(item);
            var rowsAffected = _context.SaveChanges();
            return rowsAffected > 0 ? RedirectToAction("Index") : StatusCode(StatusCodes.Status500InternalServerError);
        }

        var errors = ModelState.Values.SelectMany(v => v.Errors);
        var errorMessage = "";

        foreach (var error in errors) errorMessage += error.ErrorMessage + "\n";

        return StatusCode(StatusCodes.Status500InternalServerError, new { errorMessage });
    }

    [HttpPost]
    public IActionResult CheckUnique([FromBody] DocumentCategory? item)
    {
        if (item == null) return Json(new { isUnique = true, isValid = false });

        var isUnique = !_context.DocumentCategories.Any(source =>
            source.Id != item.Id &&
            source.Name == item.Name);

        return Json(new { isUnique, isValid = ModelState.IsValid });
    }
}