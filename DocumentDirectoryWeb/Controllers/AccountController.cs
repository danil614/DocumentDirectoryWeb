using System.Security.Claims;
using DocumentDirectoryWeb.Helpers;
using DocumentDirectoryWeb.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace DocumentDirectoryWeb.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationContext _context;
    private readonly SuperUser _superUser;

    public AccountController(ApplicationContext context, SuperUser superUser)
    {
        _context = context;
        _superUser = superUser;
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

    /// <summary>
    ///     Получает Sid и имя пользователя Windows.
    /// </summary>
    private void GetWindowsUser(out string? username, out string? userSid)
    {
        username = null;
        userSid = null;

        try
        {
            if (User.Identity is null) return;

            // Получение имени пользователя
            username = User.Identity.Name?.Split(@"\")[1];

            // Получение ключа пользователя
            userSid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.PrimarySid)?.Value;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    [HttpPost]
    public IActionResult WindowsLogin()
    {
        // Получаем имя пользователя и ключ Windows
        GetWindowsUser(out var username, out var userSid);

        // Если не удалось получить, то выводим ошибку
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userSid))
        {
            ViewBag.ErrorText = "Вход возможен только с использованием логина и пароля!";
            return View("Login");
        }

        // Получаем пользователя из базы данных
        var user = _context.Users.FirstOrDefault(u => u.Id == userSid);

        // Если пользователя нет, отправляем на регистрацию
        if (user is null)
        {
            user = new User
            {
                Id = userSid,
                Login = username
            };

            ViewBag.Departments = _context.Departments.OrderBy(d => d.Name).ToList();
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

        if (user.Login == _superUser.Username && user.Password == _superUser.Password)
        {
            SignIn(_superUser.Username, 3, "Суперпользователь");
            return RedirectToAction("Index", "Home");
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

    /// <summary>
    ///     Осуществляет вход в систему с помощью Cookie.
    /// </summary>
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
            var isFound = _context.Users.Any(u => u.Id == user.Id);

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
        ViewBag.Departments = _context.Departments.OrderBy(d => d.Name).ToList();
        return View("Registration", user);
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}