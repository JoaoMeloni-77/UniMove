using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniMove.Exceptions;
using UniMove.Models.ViewModels;
using UniMove.Repositories.Interfaces;
using UniMove.Services;

namespace UniMove.Controllers;

[Authorize(Roles = "Aluno")]
public class ReservasController : Controller
{
    private readonly IReservaRepository _repositorio;
    private readonly ReservaService _servico;

    public ReservasController(IReservaRepository repositorio, ReservaService servico)
    {
        _repositorio = repositorio;
        _servico = servico;
    }

    private int AlunoId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // Minhas reservas (histórico: passadas e futuras)
    public IActionResult Index()
    {
        return View(_repositorio.ObterReservasDoAluno(AlunoId).ToList());
    }

    public IActionResult Disponiveis()
    {
        return View(_repositorio.ObterViagensDisponiveis(AlunoId).ToList());
    }

    public IActionResult Create(int viagemId)
    {
        var viagem = _repositorio.ObterViagensDisponiveis(AlunoId)
            .FirstOrDefault(v => v.ViagemId == viagemId);

        if (viagem == null)
        {
            TempData["Erro"] = "Esta viagem não está disponível para reserva.";
            return RedirectToAction(nameof(Disponiveis));
        }
        if (viagem.AlunoJaReservou)
        {
            TempData["Erro"] = "Você já possui uma reserva ativa para esta viagem.";
            return RedirectToAction(nameof(Disponiveis));
        }

        var modelo = new NovaReservaViewModel
        {
            ViagemId = viagemId,
            Rota = viagem.Rota,
            DataHoraPartida = viagem.DataHoraPartida
        };
        PreencherPontos(modelo);
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(NovaReservaViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            PreencherPontos(modelo);
            return View(modelo);
        }

        try
        {
            _servico.Reservar(AlunoId, modelo.ViagemId, modelo.PontoEmbarqueId);
            TempData["Sucesso"] = "Reserva realizada com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (RegraNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
            return RedirectToAction(nameof(Disponiveis));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Cancelar(int id)
    {
        try
        {
            _servico.Cancelar(id, AlunoId);
            TempData["Sucesso"] = "Reserva cancelada com sucesso.";
        }
        catch (RegraNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    private void PreencherPontos(NovaReservaViewModel modelo)
    {
        modelo.Pontos = _repositorio.ObterPontosDaViagem(modelo.ViagemId)
            .Select(p => new SelectListItem(p.Nome, p.Id.ToString()))
            .ToList();
    }
}