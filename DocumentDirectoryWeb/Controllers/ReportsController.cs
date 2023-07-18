using DocumentDirectoryWeb.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DocumentDirectoryWeb.Controllers;

[Authorize(Roles = "Admin, Editor")]
public class ReportsController : Controller
{
    private readonly ApplicationContext _context;

    public ReportsController(ApplicationContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult ListByUsers()
    {
        var users = _context.Users.Include(u => u.Department)
            .Include(u => u.UserDocumentReviews)!.ThenInclude(r => r.Document)
            .ThenInclude(d => d!.Category)
            .OrderBy(u => u.FullName).ToList().AsQueryable();

        return View(users);
    }
}