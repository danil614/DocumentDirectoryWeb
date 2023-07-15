using System.Security.Claims;
using System.Security.Principal;
using DocumentDirectoryWeb.Helpers;
using DocumentDirectoryWeb.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace DocumentDirectoryWeb.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationContext _context;

    public AccountController(ApplicationContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        ViewBag.ErrorText = "У вас нет разрешения на доступ к этой странице. " +
                            "Пожалуйста, войдите под другим пользователем, который имеет соответствующие права доступа.";
        return View("Login");
    }

    [HttpPost]
    public IActionResult WindowsLogin()
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
            ViewBag.ErrorText = "Вход возможен только с использованием логина и пароля!";
            return View("Login");
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
            return View("Registration", user);
        }

        // Если пользователь есть, то входим в систему
        SignIn(user.Login, user.UserTypeId, user.FullName);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public IActionResult Login(User user)
    {
        if (string.IsNullOrEmpty(user.Login) || string.IsNullOrEmpty(user.Password))
        {
            ViewBag.ErrorText = "Логин и пароль не могут быть пустыми!";
            return View(user);
        }

        var item = _context.Users.FirstOrDefault(
            u =>
                u.Login == user.Login &&
                u.Password == HashPassword.GetHash(user.Password));

        if (item is null)
        {
            ViewBag.ErrorText = "Пользователь с указанным логином и паролем не найден!";
            return View(user);
        }

        SignIn(item.Login, item.UserTypeId, item.FullName);
        return RedirectToAction("Index", "Home");
    }

    private void SignIn(string login, int userTypeId, string fullName)
    {
        var userType = _context.UserTypes.FirstOrDefault(t => t.Id == userTypeId)?.SystemName ?? "User";
        
        var claims = new List<Claim>
        {
            new(ClaimsIdentity.DefaultNameClaimType, login),
            new(ClaimsIdentity.DefaultRoleClaimType, userType),
            new(ClaimTypes.GivenName, fullName)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme,
            ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);

        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        
        HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
    }
    
    [HttpPost]
    public IActionResult Register(User user)
    {
        if (ModelState.IsValid)
        {
            var isFound = _context.Users.Any(u => u.Id == user.Id && u.Login == user.Login);

            // Если пользователя нет, отправляем на регистрацию
            if (isFound)
            {
                _context.Users.Update(user);
            }
            else
            {
                user.UserTypeId = 1; // Обычный пользователь
                _context.Users.Add(user);
            }

            var rowsAffected = _context.SaveChanges();

            if (rowsAffected > 0)
            {
                SignIn(user.Login, user.UserTypeId, user.FullName);
                return RedirectToAction("Index", "Home");
            }
        }

        
        ViewBag.ErrorText = "Ошибка регистрации!";
        ViewBag.Departments = _context.Departments.ToList();
        return  View("Registration", user);
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}