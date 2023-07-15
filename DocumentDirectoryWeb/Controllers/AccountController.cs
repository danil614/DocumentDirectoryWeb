using System.Security.Claims;
using DocumentDirectoryWeb.Helpers;
using DocumentDirectoryWeb.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    public async Task<IActionResult> Login(User user)
    {
        if (user.Password is null)
        {
            ViewBag.ErrorText = "Пароль не может быть пустым!";
            return View(user);
        }
        
        if (ModelState.IsValid)
        {
            var item = _context.Users.Include(u => u.UserType).FirstOrDefault(
                item =>
                    item.Login == user.Login &&
                    item.Password == HashPassword.GetHash(user.Password));

            if (item is null)
            {
                ViewBag.ErrorText = "Пользователь с указанным логином и паролем не найден!";
                return View(user);
            }

            var claimsPrincipal = GetClaimsPrincipal(user.Login, user.UserType?.Name!, user.FullName);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            return RedirectToAction("Index", "Home");
        }

        return View(user);
    }

    public static ClaimsPrincipal GetClaimsPrincipal(string login, string userType, string fullName)
    {
        var claims = new List<Claim>
        {
            new(ClaimsIdentity.DefaultNameClaimType, login),
            new(ClaimsIdentity.DefaultRoleClaimType, userType),
            new(ClaimTypes.GivenName, fullName)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme,
            ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);

        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        return claimsPrincipal;
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
            return rowsAffected > 0 ? RedirectToAction("Index", "Home") : StatusCode(StatusCodes.Status500InternalServerError);
        }

        return View("Registration", user);
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}