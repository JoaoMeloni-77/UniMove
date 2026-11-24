using Microsoft.AspNetCore.Mvc;
using UniMove.Exceptions;
using UniMove.Models;
using UniMove.Models.ViewModels;
using UniMove.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace UniMove.Controllers;

[Authorize(Roles = "Administrador")]
public class MotoristasController : Controller
{
    private const string SenhaProvisoria = "HASH_PROVISORIO";

    private readonly IMotoristaRepository _repositorio;

    public MotoristasController(IMotoristaRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public IActionResult Index()
    {
        var lista = _repositorio.ObterTodos()
            .Select(m => new MotoristaViewModel(m))
            .ToList();
        return View(lista);
    }

    public IActionResult Details(int id)
    {
        var motorista = _repositorio.ObterPorId(id);
        if (motorista == null)
            return NaoEncontrado();

        return View(new MotoristaViewModel(motorista));
    }

    public IActionResult Create()
    {
        return View(new MotoristaViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(MotoristaViewModel modelo)
    {
        if (!ModelState.IsValid)
            return View(modelo);

        try
        {
            var motorista = new Motorista(modelo.Nome, modelo.Email, SenhaProvisoria,
                                          modelo.Cnh, modelo.Telefone);
            _repositorio.Adicionar(motorista);

            TempData["Sucesso"] = "Motorista cadastrado com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (RegraNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(modelo);
        }
    }

    public IActionResult Edit(int id)
    {
        var motorista = _repositorio.ObterPorId(id);
        if (motorista == null)
            return NaoEncontrado();

        return View(new MotoristaViewModel(motorista));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, MotoristaViewModel modelo)
    {
        if (id != modelo.Id)
            return NaoEncontrado();

        if (!ModelState.IsValid)
            return View(modelo);

        try
        {
            var motorista = _repositorio.ObterPorId(id);
            if (motorista == null)
                return NaoEncontrado();

            motorista.AlterarDados(modelo.Nome, modelo.Email, modelo.Cnh, modelo.Telefone);
            _repositorio.Atualizar(motorista);

            TempData["Sucesso"] = "Motorista atualizado com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (RegraNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(modelo);
        }
    }

    public IActionResult Delete(int id)
    {
        var motorista = _repositorio.ObterPorId(id);
        if (motorista == null)
            return NaoEncontrado();

        return View(new MotoristaViewModel(motorista));
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmado(int id)
    {
        try
        {
            _repositorio.Remover(id);
            TempData["Sucesso"] = "Motorista excluído com sucesso.";
        }
        catch (RegraNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private IActionResult NaoEncontrado()
    {
        TempData["Erro"] = "Motorista não encontrado.";
        return RedirectToAction(nameof(Index));
    }
}