using System.Diagnostics;
using System.Security.Principal;
using Microsoft.AspNetCore.Mvc;
using DocumentDirectoryWeb.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace DocumentDirectoryWeb.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationContext _context;

    public HomeController(ApplicationContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        string? username = null;
        string? userSid = null;
        
        // Получаем имя пользователя и ключ Windows
        try
        {
#pragma warning disable CA1416
            // Получение текущего пользователя
            var currentIdentity = WindowsIdentity.GetCurrent();
            // Получение имени пользователя
            username = currentIdentity.Name.Split(@"\")[1];
            // Получение ключа пользователя
            userSid = currentIdentity.User?.Value;
#pragma warning restore CA1416
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        // Если не удалось получить, то выводим ошибку
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userSid))
        {
            // TODO: нужна ли реализация не для Windows?
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        // Получаем пользователя из базы данных
        var user = _context.Users.FirstOrDefault(u => u.Id == userSid && u.Login == username);

        // Если пользователя нет, отправляем на регистрацию
        if (user is null)
        {
            user = new User
            {
                Id = userSid,
                Login = username
            };

            ViewBag.Departments = _context.Departments.ToList();
                
            return View("~/Views/Account/Registration.cshtml", user);
        }

        // Если пользователь есть, то получаем роль
        var userType = _context.UserTypes.FirstOrDefault(t => t.Id == user.UserTypeId)?.SystemName ?? "User";

        var claimsPrincipal = AccountController.GetClaimsPrincipal(user.Login, userType, user.FullName);
        HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
        
        // Перенаправляем пользователя по роли
        switch (userType)
        {
            case "User":
                return RedirectToAction("Index", "DocumentView");
            case "Editor":
            case "Admin":
                return RedirectToAction("Index", "DocumentManagement");
            default:
                return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}