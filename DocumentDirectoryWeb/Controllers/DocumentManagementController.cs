using Microsoft.AspNetCore.Mvc;

namespace DocumentDirectoryWeb.Controllers;

public class DocumentManagementController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}