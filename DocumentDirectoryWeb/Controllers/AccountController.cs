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

    [HttpPost]
    public IActionResult Registration()
    {
        var user = new User
        {
            Id = Guid.NewGuid().ToString()
        };

        ViewBag.Departments = _context.Departments.OrderBy(d => d.Name).ToList();
        return View(user);
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
            SignIn(_superUser.Username, _superUser.Username, 3, "Суперпользователь");
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

        SignIn(item.Id, item.Login, item.UserTypeId, item.FullName);
        return RedirectToAction("Index", "Home");
    }

    /// <summary>
    ///     Осуществляет вход в систему с помощью Cookie.
    /// </summary>
    private void SignIn(string id, string login, int userTypeId, string fullName)
    {
        var userType = _context.UserTypes.FirstOrDefault(t => t.Id == userTypeId)?.SystemName ?? "User";

        var claims = new List<Claim>
        {
            new(ClaimsIdentity.DefaultNameClaimType, login),
            new(ClaimsIdentity.DefaultRoleClaimType, userType),
            new(ClaimTypes.GivenName, fullName),
            new(ClaimTypes.System, id)
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
            user.UserTypeId = 1; // Обычный пользователь
            user.Password = HashPassword.GetHash(user.Password);
            _context.Users.Add(user);

            var rowsAffected = _context.SaveChanges();

            if (rowsAffected > 0)
            {
                SignIn(user.Id, user.Login, user.UserTypeId, user.FullName);
                return RedirectToAction("Index", "Home");
            }
        }

        ViewBag.ErrorText = "Ошибка при регистрации. Поля неправильно заполнены!";
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