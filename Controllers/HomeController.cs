using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniMove.Models;

namespace UniMove.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        var painel = User.FindFirstValue("PainelInicial");
        return string.IsNullOrEmpty(painel)
            ? RedirectToAction("Login", "Account")
            : LocalRedirect(painel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}