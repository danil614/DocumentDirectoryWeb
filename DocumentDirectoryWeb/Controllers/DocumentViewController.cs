using Microsoft.AspNetCore.Mvc;

namespace DocumentDirectoryWeb.Controllers;

public class DocumentViewController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}