using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniMove.Models.ViewModels;
using UniMove.Repositories.Interfaces;

namespace UniMove.Controllers;

[Authorize(Roles = "Administrador")]
public class DashboardController : Controller
{
    private readonly IDashboardRepository _repositorio;

    public DashboardController(IDashboardRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public IActionResult Index()
    {
        var modelo = new DashboardViewModel
        {
            Indicadores = _repositorio.ObterIndicadores(),
            Ocupacao = _repositorio.ObterOcupacaoPorViagem().ToList()
        };
        return View(modelo);
    }
}