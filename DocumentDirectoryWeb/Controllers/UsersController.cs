﻿using DocumentDirectoryWeb.Helpers;
using DocumentDirectoryWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DocumentDirectoryWeb.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly ApplicationContext _context;

    public UsersController(ApplicationContext context)
    {
        _context = context;
    }

    private IQueryable<User> GetData()
    {
        return _context.Users.Include(u => u.Department).Include(u => u.UserType);
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(GetData().OrderBy(item => item.Login).ToList().AsQueryable());
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult DeleteItem(string id)
    {
        var item = _context.Users.FirstOrDefault(item => item.Id == id);
        if (item == null) return NotFound(); // Если запись не найдена, возвращаем ошибку 404

        _context.Users.Remove(item);

        var rowsAffected = _context.SaveChanges();
        return rowsAffected > 0 ? Ok() : StatusCode(StatusCodes.Status500InternalServerError);
    }

    [HttpGet]
    public IActionResult GetItem(string id)
    {
        var item = _context.Users.FirstOrDefault(item => item.Id == id);
        if (item == null) return NotFound(); // Если запись не найдена, возвращаем ошибку 404

        ViewBag.Edit = true;
        if (item.Password != null) ViewBag.OldPassword = item.Password;
        ViewBag.UserTypes = _context.UserTypes.OrderBy(t => t.Name).ToList();
        ViewBag.Departments = _context.Departments.OrderBy(d => d.Name).ToList();

        return PartialView("Form", item);
    }

    [HttpGet]
    public IActionResult CreateItem()
    {
        var item = new User
        {
            Id = Guid.NewGuid().ToString()
        };

        ViewBag.Edit = false;
        ViewBag.UserTypes = _context.UserTypes.OrderBy(t => t.Name).ToList();
        ViewBag.Departments = _context.Departments.OrderBy(d => d.Name).ToList();

        return PartialView("Form", item);
    }

    [HttpPost]
    public IActionResult SaveItem(User item, bool isEdit, string? oldPassword)
    {
        if (ModelState.IsValid)
        {
            if (isEdit) // Если редактирование пользователя
            {
                if (item.Password != oldPassword) // Если пароль изменился, то хешируем его.
                    item.Password = HashPassword.GetHash(item.Password);

                _context.Users.Update(item);
            }
            else
            {
                item.Password = HashPassword.GetHash(item.Password);
                _context.Users.Add(item);
            }

            var rowsAffected = _context.SaveChanges();
            return rowsAffected > 0 ? RedirectToAction("Index") : StatusCode(StatusCodes.Status500InternalServerError);
        }

        var errors = ModelState.Values.SelectMany(v => v.Errors);
        var errorMessage = "";

        foreach (var error in errors) errorMessage += error.ErrorMessage + "\n";

        return StatusCode(StatusCodes.Status500InternalServerError, new { errorMessage });
    }

    [HttpPost]
    [AllowAnonymous]
    public IActionResult CheckUnique([FromBody] User? item)
    {
        if (item == null) return Json(new { isUnique = true, isValid = false });

        var isUnique = !_context.Users.Any(source =>
            source.Id != item.Id &&
            source.Login == item.Login &&
            source.DepartmentId == item.DepartmentId);

        return Json(new { isUnique, isValid = ModelState.IsValid });
    }
}