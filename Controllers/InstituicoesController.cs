using Microsoft.AspNetCore.Mvc;
using UniMove.Exceptions;
using UniMove.Models;
using UniMove.Models.ViewModels;
using UniMove.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace UniMove.Controllers;

[Authorize(Roles = "Administrador")]
public class InstituicoesController : Controller
{
    private readonly IInstituicaoRepository _repositorio;

    public InstituicoesController(IInstituicaoRepository repositorio)
    {
        _repositorio = repositorio;
    }

    // GET: /Instituicoes
    public IActionResult Index()
    {
        var lista = _repositorio.ObterTodos()
            .Select(i => new InstituicaoViewModel(i))
            .ToList();
        return View(lista);
    }

    // GET: /Instituicoes/Details/5
    public IActionResult Details(int id)
    {
        var instituicao = _repositorio.ObterPorId(id);
        if (instituicao == null)
            return NaoEncontrada();

        return View(new InstituicaoViewModel(instituicao));
    }

    // GET: /Instituicoes/Create
    public IActionResult Create()
    {
        return View(new InstituicaoViewModel());
    }

    // POST: /Instituicoes/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(InstituicaoViewModel modelo)
    {
        if (!ModelState.IsValid)
            return View(modelo);

        try
        {
            var instituicao = new Instituicao(modelo.Nome, modelo.Cidade);
            _repositorio.Adicionar(instituicao);

            TempData["Sucesso"] = "Instituição cadastrada com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (RegraNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(modelo);
        }
    }

    // GET: /Instituicoes/Edit/5
    public IActionResult Edit(int id)
    {
        var instituicao = _repositorio.ObterPorId(id);
        if (instituicao == null)
            return NaoEncontrada();

        return View(new InstituicaoViewModel(instituicao));
    }

    // POST: /Instituicoes/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, InstituicaoViewModel modelo)
    {
        if (id != modelo.Id)
            return NaoEncontrada();

        if (!ModelState.IsValid)
            return View(modelo);

        try
        {
            var instituicao = _repositorio.ObterPorId(id);
            if (instituicao == null)
                return NaoEncontrada();

            instituicao.AlterarDados(modelo.Nome, modelo.Cidade);
            _repositorio.Atualizar(instituicao);

            TempData["Sucesso"] = "Instituição atualizada com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (RegraNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(modelo);
        }
    }

    // GET: /Instituicoes/Delete/5
    public IActionResult Delete(int id)
    {
        var instituicao = _repositorio.ObterPorId(id);
        if (instituicao == null)
            return NaoEncontrada();

        return View(new InstituicaoViewModel(instituicao));
    }

    // POST: /Instituicoes/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmado(int id)
    {
        try
        {
            _repositorio.Remover(id);
            TempData["Sucesso"] = "Instituição excluída com sucesso.";
        }
        catch (RegraNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private IActionResult NaoEncontrada()
    {
        TempData["Erro"] = "Instituição não encontrada.";
        return RedirectToAction(nameof(Index));
    }
}