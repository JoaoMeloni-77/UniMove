using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniMove.Models.Enums;

namespace UniMove.Models.ViewModels;

// Dados da viagem usados pelo ReservaService para aplicar as regras
public class ViagemParaReserva
{
    public int Id { get; set; }
    public int RotaId { get; set; }
    public DateTime DataHoraPartida { get; set; }
    public StatusViagem Status { get; set; }
    public int Capacidade { get; set; }
    public int ReservasAtivas { get; set; }
    public int VagasDisponiveis => Capacidade - ReservasAtivas;
}

public class ViagemDisponivelViewModel
{
    public int ViagemId { get; set; }
    public DateTime DataHoraPartida { get; set; }
    public TipoViagem Tipo { get; set; }
    public string Rota { get; set; } = string.Empty;
    public int Capacidade { get; set; }
    public int ReservasAtivas { get; set; }
    public int JaReservou { get; set; }

    public int Vagas => Capacidade - ReservasAtivas;
    public bool AlunoJaReservou => JaReservou > 0;
}

public class MinhaReservaViewModel
{
    public int ReservaId { get; set; }
    public int ViagemId { get; set; }
    public DateTime DataHoraPartida { get; set; }
    public TipoViagem Tipo { get; set; }
    public StatusViagem StatusViagem { get; set; }
    public string Rota { get; set; } = string.Empty;
    public string Ponto { get; set; } = string.Empty;
    public StatusReserva StatusReserva { get; set; }

    public bool PodeCancelar =>
        StatusReserva == StatusReserva.Confirmada
        && StatusViagem == StatusViagem.Agendada
        && DataHoraPartida > DateTime.Now;
}

public class PassageiroViewModel
{
    public int ReservaId { get; set; }
    public string AlunoNome { get; set; } = string.Empty;
    public string Matricula { get; set; } = string.Empty;
    public string Ponto { get; set; } = string.Empty;
    public StatusReserva Status { get; set; }
}

public class PontoOpcao
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
}

public class NovaReservaViewModel
{
    public int ViagemId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Escolha um ponto de embarque.")]
    [Display(Name = "Ponto de embarque")]
    public int PontoEmbarqueId { get; set; }

    [ValidateNever] public string Rota { get; set; } = string.Empty;
    [ValidateNever] public DateTime DataHoraPartida { get; set; }
    [ValidateNever] public List<SelectListItem> Pontos { get; set; } = new();
}