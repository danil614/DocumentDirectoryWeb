using System.Diagnostics;
using System.Security.Principal;
using DocumentDirectoryWeb.Helpers;
using Microsoft.AspNetCore.Mvc;
using DocumentDirectoryWeb.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

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

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}