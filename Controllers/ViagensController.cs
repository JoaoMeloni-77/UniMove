using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniMove.Exceptions;
using UniMove.Models;
using UniMove.Models.ViewModels;
using UniMove.Repositories.Interfaces;
using UniMove.Services;

namespace UniMove.Controllers;

public class ViagensController : Controller
{
    private readonly IViagemRepository _repositorio;
    private readonly IViagemService _servico;
    private readonly IRotaRepository _rotas;
    private readonly IMotoristaRepository _motoristas;
    private readonly IVeiculoRepository _veiculos;

    public ViagensController(
        IViagemRepository repositorio,
        IViagemService servico,
        IRotaRepository rotas,
        IMotoristaRepository motoristas,
        IVeiculoRepository veiculos)
    {
        _repositorio = repositorio;
        _servico = servico;
        _rotas = rotas;
        _motoristas = motoristas;
        _veiculos = veiculos;
    }

    // GET: /Viagens
    public IActionResult Index()
    {
        return View(MontarLista(_repositorio.ObterTodos()));
    }

    // GET: /Viagens/Details/5
    public IActionResult Details(int id)
    {
        var viagem = _repositorio.ObterPorId(id);
        if (viagem == null)
            return NaoEncontrada();

        return View(MontarUma(viagem));
    }

    // GET: /Viagens/Create
    public IActionResult Create()
    {
        var viewModel = new ViagemViewModel();
        PreencherListas(viewModel);
        return View(viewModel);
    }

    // POST: /Viagens/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(ViagemViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            PreencherListas(viewModel);
            return View(viewModel);
        }

        try
        {
            var viagem = new Viagem(viewModel.RotaId, viewModel.MotoristaId, viewModel.VeiculoId,
                                    viewModel.DataHoraPartida, viewModel.Tipo);
            _servico.Cadastrar(viagem);

            TempData["Sucesso"] = "Viagem cadastrada com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (RegraNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            PreencherListas(viewModel);
            return View(viewModel);
        }
    }

    // GET: /Viagens/Edit/5
    public IActionResult Edit(int id)
    {
        var viagem = _repositorio.ObterPorId(id);
        if (viagem == null)
            return NaoEncontrada();

        if (viagem.Status != Models.Enums.StatusViagem.Agendada)
        {
            TempData["Erro"] = "Somente viagens agendadas podem ser alteradas.";
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new ViagemViewModel(viagem);
        PreencherListas(viewModel);
        return View(viewModel);
    }

    // POST: /Viagens/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, ViagemViewModel viewModel)
    {
        if (id != viewModel.Id)
            return NaoEncontrada();

        if (!ModelState.IsValid)
        {
            PreencherListas(viewModel);
            return View(viewModel);
        }

        try
        {
            var viagem = _repositorio.ObterPorId(id);
            if (viagem == null)
                return NaoEncontrada();

            viagem.AlterarDados(viewModel.RotaId, viewModel.MotoristaId, viewModel.VeiculoId,
                                viewModel.DataHoraPartida, viewModel.Tipo);
            _servico.Alterar(viagem);

            TempData["Sucesso"] = "Viagem atualizada com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (RegraNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            PreencherListas(viewModel);
            return View(viewModel);
        }
    }

    // POST: /Viagens/Cancelar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Cancelar(int id)
    {
        try
        {
            var viagem = _repositorio.ObterPorId(id);
            if (viagem == null)
                return NaoEncontrada();

            viagem.Cancelar();
            _repositorio.Atualizar(viagem);

            TempData["Sucesso"] = "Viagem cancelada.";
        }
        catch (RegraNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: /Viagens/Delete/5
    public IActionResult Delete(int id)
    {
        var viagem = _repositorio.ObterPorId(id);
        if (viagem == null)
            return NaoEncontrada();

        return View(MontarUma(viagem));
    }

    // POST: /Viagens/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmado(int id)
    {
        try
        {
            _repositorio.Remover(id);
            TempData["Sucesso"] = "Viagem excluída com sucesso.";
        }
        catch (RegraNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    // Preenche as caixas de seleção do formulário
    private void PreencherListas(ViagemViewModel viewModel)
    {
        viewModel.Rotas = _rotas.ObterTodos()
            .Select(r => new SelectListItem($"{r.Nome} ({r.DuracaoMinutos} min)", r.Id.ToString()))
            .ToList();

        viewModel.Motoristas = _motoristas.ObterTodos()
            .Where(m => m.Ativo)
            .Select(m => new SelectListItem(m.Nome, m.Id.ToString()))
            .ToList();

        viewModel.Veiculos = _veiculos.ObterTodos()
            .Select(v => new SelectListItem($"{v.Placa} - {v.Modelo} ({v.Capacidade} lugares)", v.Id.ToString()))
            .ToList();
    }

    // Monta os ViewModels já com os nomes de rota, motorista e veículo
    private List<ViagemViewModel> MontarLista(IEnumerable<Viagem> viagens)
    {
        var rotas = _rotas.ObterTodos().ToDictionary(r => r.Id);
        var motoristas = _motoristas.ObterTodos().ToDictionary(m => m.Id);
        var veiculos = _veiculos.ObterTodos().ToDictionary(v => v.Id);

        return viagens.Select(viagem =>
        {
            var viewModel = new ViagemViewModel(viagem);

            if (rotas.TryGetValue(viagem.RotaId, out var rota))
            {
                viewModel.RotaNome = rota.Nome;
                viewModel.ChegadaPrevista = viagem.CalcularChegadaPrevista(rota.DuracaoMinutos);
            }

            if (motoristas.TryGetValue(viagem.MotoristaId, out var motorista))
                viewModel.MotoristaNome = motorista.Nome;

            if (veiculos.TryGetValue(viagem.VeiculoId, out var veiculo))
            {
                viewModel.VeiculoDescricao = $"{veiculo.Placa} - {veiculo.Modelo}";
                viewModel.Capacidade = veiculo.Capacidade;
            }

            viewModel.ReservasAtivas = _repositorio.ContarReservasAtivas(viagem.Id);
            return viewModel;
        }).ToList();
    }

    private ViagemViewModel MontarUma(Viagem viagem)
    {
        return MontarLista(new[] { viagem }).First();
    }

    private IActionResult NaoEncontrada()
    {
        TempData["Erro"] = "Viagem não encontrada.";
        return RedirectToAction(nameof(Index));
    }
}