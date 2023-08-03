using DocumentDirectoryWeb.Helpers;
using DocumentDirectoryWeb.Models;
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
        return View();
    }

    [HttpGet]
    public IActionResult ListByDocuments()
    {
        return View();
    }

    [HttpGet]
    public IActionResult UserDocumentReviews()
    {
        return View();
    }

    [HttpPost]
    public IActionResult GetListByUsers()
    {
        var users = _context.Users.Include(u => u.Department)
            .Include(u => u.UserDocumentReviews)!.ThenInclude(r => r.Document)
            .ThenInclude(d => d!.Categories)
            .OrderBy(u => u.Login).ToList();

        var userDataList = users.Select(
            user => new
            {
                user.Id,
                user.Login,
                Department = user.Department?.Name,
                Reviews = (user.UserDocumentReviews ?? new List<UserDocumentReview>()).Select(review =>
                    new
                    {
                        review.Document!.Name,
                        Categories = review.Document.GetCategories(),
                        review.IsReviewed,
                        review.ReviewDate
                    }).OrderBy(d => d.Name).AsEnumerable()
            });

        return Json(userDataList);
    }

    [HttpPost]
    public IActionResult GetListByDocuments()
    {
        var documents = _context.Documents.Include(d => d.Categories)
            .Include(d => d.UserDocumentReviews)!.ThenInclude(r => r.User)
            .ThenInclude(u => u!.Department)
            .OrderBy(d => d.Name).ToList();

        var documentDataList = documents.Select(
            document => new
            {
                document.Id,
                document.Name,
                Categories = document.GetCategories(),
                Reviews = (document.UserDocumentReviews ?? new List<UserDocumentReview>()).Select(review =>
                    new
                    {
                        review.User?.Login,
                        Department = review.User?.Department!.Name,
                        review.IsReviewed,
                        review.ReviewDate
                    }).OrderBy(u => u.Login).AsEnumerable()
            });

        return Json(documentDataList);
    }

    [HttpPost]
    public IActionResult GetUserDocumentReviews()
    {
        var userDocumentReviews = _context.UserDocumentReviews
            .Include(r => r.Document).ThenInclude(d => d!.Categories)
            .Include(r => r.User).ThenInclude(u => u!.Department)
            .Select(review => new
            {
                review.Document!.Name,
                Categories = review.Document.Categories!.Select(c => new { c.Name }),
                review.User!.Login,
                Department = review.User.Department!.Name,
                review.IsReviewed,
                review.ReviewDate
            });

        return Json(userDocumentReviews);
    }
}