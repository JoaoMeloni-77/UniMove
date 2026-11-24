using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniMove.Models.ViewModels;
using UniMove.Repositories.Interfaces;

namespace UniMove.Controllers;

[Authorize(Roles = "Administrador")]
public class RelatoriosController : Controller
{
    private readonly IDashboardRepository _repositorio;
    private readonly IInstituicaoRepository _instituicoes;

    public RelatoriosController(IDashboardRepository repositorio, IInstituicaoRepository instituicoes)
    {
        _repositorio = repositorio;
        _instituicoes = instituicoes;
    }

    public IActionResult Index(int instituicaoId = 0, DateTime? dataInicial = null,
                               DateTime? dataFinal = null, string turno = "todos")
    {
        var modelo = new RelatorioViewModel
        {
            InstituicaoId = instituicaoId,
            DataInicial = dataInicial,
            DataFinal = dataFinal,
            Turno = turno,
            Instituicoes = _instituicoes.ObterTodos()
                .Select(i => new SelectListItem(i.Nome, i.Id.ToString()))
                .ToList()
        };

        if (dataInicial.HasValue && dataFinal.HasValue && dataInicial > dataFinal)
        {
            ModelState.AddModelError(string.Empty, "A data inicial não pode ser maior que a data final.");
            return View(modelo);
        }

        var (horaMin, horaMax) = turno switch
        {
            "manha" => (0, 11),
            "tarde" => (12, 17),
            "noite" => (18, 23),
            _ => (0, 23)
        };

        modelo.Linhas = _repositorio
            .ObterRelatorio(instituicaoId, dataInicial, dataFinal, horaMin, horaMax)
            .ToList();
        return View(modelo);
    }
}