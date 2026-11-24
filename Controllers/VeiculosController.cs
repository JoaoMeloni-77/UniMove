using Microsoft.AspNetCore.Mvc;
using UniMove.Exceptions;
using UniMove.Models;
using UniMove.Models.ViewModels;
using UniMove.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace UniMove.Controllers;

[Authorize(Roles = "Administrador")]
public class VeiculosController : Controller
{
    private readonly IVeiculoRepository _repositorio;

    public VeiculosController(IVeiculoRepository repositorio)
    {
        _repositorio = repositorio;
    }

    // GET: /Veiculos
    public IActionResult Index()
    {
        var lista = _repositorio.ObterTodos()
            .Select(v => new VeiculoViewModel(v))
            .ToList();
        return View(lista);
    }

    // GET: /Veiculos/Details/5
    public IActionResult Details(int id)
    {
        var veiculo = _repositorio.ObterPorId(id);
        if (veiculo == null)
            return NaoEncontrado();

        return View(new VeiculoViewModel(veiculo));
    }

    // GET: /Veiculos/Create
    public IActionResult Create()
    {
        return View(new VeiculoViewModel());
    }

    // POST: /Veiculos/Create
        // POST: /Veiculos/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(VeiculoViewModel viewModel)
    {
        if (!ModelState.IsValid)
            return View(viewModel);

        try
        {
            var veiculo = new Veiculo(viewModel.Placa, viewModel.Modelo, viewModel.Capacidade);
            _repositorio.Adicionar(veiculo);

            TempData["Sucesso"] = "Veículo cadastrado com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (RegraNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(viewModel);
        }
    }

    // GET: /Veiculos/Edit/5
    public IActionResult Edit(int id)
    {
        var veiculo = _repositorio.ObterPorId(id);
        if (veiculo == null)
            return NaoEncontrado();

        return View(new VeiculoViewModel(veiculo));
    }

      // POST: /Veiculos/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, VeiculoViewModel viewModel)
    {
        if (id != viewModel.Id)
            return NaoEncontrado();

        if (!ModelState.IsValid)
            return View(viewModel);

        try
        {
            var veiculo = _repositorio.ObterPorId(id);
            if (veiculo == null)
                return NaoEncontrado();

            veiculo.AlterarDados(viewModel.Placa, viewModel.Modelo, viewModel.Capacidade);
            _repositorio.Atualizar(veiculo);

            TempData["Sucesso"] = "Veículo atualizado com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (RegraNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(viewModel);
        }
    }

    // GET: /Veiculos/Delete/5
    public IActionResult Delete(int id)
    {
        var veiculo = _repositorio.ObterPorId(id);
        if (veiculo == null)
            return NaoEncontrado();

        return View(new VeiculoViewModel(veiculo));
    }

    // POST: /Veiculos/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmado(int id)
    {
        try
        {
            _repositorio.Remover(id);
            TempData["Sucesso"] = "Veículo excluído com sucesso.";
        }
        catch (RegraNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private IActionResult NaoEncontrado()
    {
        TempData["Erro"] = "Veículo não encontrado.";
        return RedirectToAction(nameof(Index));
    }
}