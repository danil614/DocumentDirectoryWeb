using System.Diagnostics;
using DocumentDirectoryWeb.Helpers;
using DocumentDirectoryWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace DocumentDirectoryWeb.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        var userType = UserTabManager.GetUserType(User.Claims);

        // Перенаправляем пользователя по роли
        switch (userType)
        {
            case "User":
                return RedirectToAction("Index", "DocumentView");
            case "Editor":
            case "Admin":
                return RedirectToAction("Index", "DocumentManagement");
            default:
                return RedirectToAction("Login", "Account");
        }
    }

    public IActionResult DeveloperInfo()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}