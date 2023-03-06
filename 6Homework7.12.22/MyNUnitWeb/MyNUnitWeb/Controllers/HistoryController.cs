namespace MyNUnitWeb.Controllers;

using Microsoft.AspNetCore.Mvc;

public class HistoryController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}