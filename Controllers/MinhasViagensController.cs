using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniMove.Exceptions;
using UniMove.Models.ViewModels;
using UniMove.Repositories.Interfaces;
using UniMove.Services;

namespace UniMove.Controllers;

[Authorize(Roles = "Motorista")]
public class MinhasViagensController : Controller
{
    private readonly IViagemRepository _viagens;
    private readonly OperacaoViagemService _operacao;

    public MinhasViagensController(IViagemRepository viagens, OperacaoViagemService operacao)
    {
        _viagens = viagens;
        _operacao = operacao;
    }

    // O Id do motorista vem do cookie de login, nunca do formulário
    private int MotoristaLogadoId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // GET: /MinhasViagens
    public IActionResult Index()
    {
        return View(_viagens.ObterViagensDoMotorista(MotoristaLogadoId).ToList());
    }

    // GET: /MinhasViagens/Details/5
    public IActionResult Details(int id)
    {
        var viagem = CarregarViagemDoMotorista(id);
        if (viagem == null)
            return NaoEncontrada();

        return View(viagem);
    }

    // POST: /MinhasViagens/Iniciar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Iniciar(int id)
    {
        try
        {
            _operacao.Iniciar(id, MotoristaLogadoId);
            TempData["Sucesso"] = "Viagem iniciada.";
        }
        catch (RegraNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: /MinhasViagens/Finalizar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Finalizar(int id)
    {
        try
        {
            _operacao.Finalizar(id, MotoristaLogadoId);
            TempData["Sucesso"] = "Viagem finalizada.";
        }
        catch (RegraNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: /MinhasViagens/ConfirmarEmbarque/5?reservaId=9
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ConfirmarEmbarque(int id, int reservaId)
    {
        try
        {
            _operacao.ConfirmarEmbarque(reservaId, MotoristaLogadoId);
            TempData["Sucesso"] = "Embarque confirmado.";
        }
        catch (RegraNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // Só devolve a viagem se ela for do motorista logado
    private ViagemMotoristaViewModel? CarregarViagemDoMotorista(int id)
    {
        var viagem = _viagens.ObterResumo(id);
        if (viagem == null || viagem.MotoristaId != MotoristaLogadoId)
            return null;

        viagem.Passageiros = _viagens.ObterPassageiros(id).ToList();
        return viagem;
    }

    private IActionResult NaoEncontrada()
    {
        TempData["Erro"] = "Viagem não encontrada.";
        return RedirectToAction(nameof(Index));
    }
}