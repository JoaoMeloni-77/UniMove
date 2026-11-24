using Microsoft.AspNetCore.Mvc;
using UniMove.Exceptions;
using UniMove.Models;
using UniMove.Models.ViewModels;
using UniMove.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace UniMove.Controllers;

[Authorize(Roles = "Administrador")]
public class RotasController : Controller
{
    private readonly IRotaRepository _repositorio;

    public RotasController(IRotaRepository repositorio)
    {
        _repositorio = repositorio;
    }

    // GET: /Rotas
    public IActionResult Index()
    {
        var lista = _repositorio.ObterTodos()
            .Select(r => new RotaViewModel(r))
            .ToList();
        return View(lista);
    }

    // GET: /Rotas/Details/5
    public IActionResult Details(int id)
    {
        var rota = _repositorio.ObterPorId(id);
        if (rota == null)
            return NaoEncontrada();

        return View(new RotaViewModel(rota));
    }

    // GET: /Rotas/Create
    public IActionResult Create()
    {
        return View(new RotaViewModel());
    }

    // POST: /Rotas/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(RotaViewModel viewModel)
    {
        if (!ModelState.IsValid)
            return View(viewModel);

        try
        {
            var rota = new Rota(viewModel.Nome, viewModel.DuracaoMinutos);
            var id = _repositorio.Adicionar(rota);

            TempData["Sucesso"] = "Rota cadastrada. Agora adicione os pontos de embarque.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (RegraNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(viewModel);
        }
    }

    // GET: /Rotas/Edit/5
    public IActionResult Edit(int id)
    {
        var rota = _repositorio.ObterPorId(id);
        if (rota == null)
            return NaoEncontrada();

        return View(new RotaViewModel(rota));
    }

    // POST: /Rotas/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, RotaViewModel viewModel)
    {
        if (id != viewModel.Id)
            return NaoEncontrada();

        if (!ModelState.IsValid)
            return View(viewModel);

        try
        {
            var rota = _repositorio.ObterPorId(id);
            if (rota == null)
                return NaoEncontrada();

            rota.AlterarDados(viewModel.Nome, viewModel.DuracaoMinutos);
            _repositorio.Atualizar(rota);

            TempData["Sucesso"] = "Rota atualizada com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (RegraNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(viewModel);
        }
    }

    // POST: /Rotas/AdicionarPonto?rotaId=5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AdicionarPonto(int rotaId, PontoEmbarqueViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            TempData["Erro"] = PrimeiroErro();
            return RedirectToAction(nameof(Details), new { id = rotaId });
        }

        try
        {
            var rota = _repositorio.ObterPorId(rotaId);
            if (rota == null)
                return NaoEncontrada();

            rota.AdicionarPonto(viewModel.Nome, viewModel.Referencia);
            _repositorio.Atualizar(rota);

            TempData["Sucesso"] = "Ponto de embarque adicionado.";
        }
        catch (RegraNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id = rotaId });
    }

    // POST: /Rotas/RemoverPonto?rotaId=5&pontoId=9
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RemoverPonto(int rotaId, int pontoId)
    {
        try
        {
            var rota = _repositorio.ObterPorId(rotaId);
            if (rota == null)
                return NaoEncontrada();

            rota.RemoverPonto(pontoId);
            _repositorio.Atualizar(rota);

            TempData["Sucesso"] = "Ponto de embarque removido.";
        }
        catch (RegraNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id = rotaId });
    }

    // GET: /Rotas/Delete/5
    public IActionResult Delete(int id)
    {
        var rota = _repositorio.ObterPorId(id);
        if (rota == null)
            return NaoEncontrada();

        return View(new RotaViewModel(rota));
    }

    // POST: /Rotas/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmado(int id)
    {
        try
        {
            _repositorio.Remover(id);
            TempData["Sucesso"] = "Rota excluída com sucesso.";
        }
        catch (RegraNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private string PrimeiroErro()
    {
        return ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .FirstOrDefault() ?? "Dados inválidos.";
    }

    private IActionResult NaoEncontrada()
    {
        TempData["Erro"] = "Rota não encontrada.";
        return RedirectToAction(nameof(Index));
    }
}