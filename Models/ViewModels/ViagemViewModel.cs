using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniMove.Models;
using UniMove.Models.Enums;

namespace UniMove.Models.ViewModels;

public class ViagemViewModel
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecione a rota.")]
    [Display(Name = "Rota")]
    public int RotaId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecione o motorista.")]
    [Display(Name = "Motorista")]
    public int MotoristaId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecione o veículo.")]
    [Display(Name = "Veículo")]
    public int VeiculoId { get; set; }

    [Required(ErrorMessage = "Informe a data e a hora de partida.")]
    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
    [Display(Name = "Data e hora de partida")]
    public DateTime DataHoraPartida { get; set; } = DateTime.Today.AddDays(1).AddHours(6).AddMinutes(30);

    [Display(Name = "Tipo")]
    public TipoViagem Tipo { get; set; }

    // --- Somente para exibir ---
    public StatusViagem Status { get; set; }
    public string RotaNome { get; set; } = string.Empty;
    public string MotoristaNome { get; set; } = string.Empty;
    public string VeiculoDescricao { get; set; } = string.Empty;
    public DateTime? ChegadaPrevista { get; set; }
    public int ReservasAtivas { get; set; }
    public int Capacidade { get; set; }

    // --- Listas das caixas de seleção ---
    public IEnumerable<SelectListItem> Rotas { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Motoristas { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Veiculos { get; set; } = new List<SelectListItem>();

    public bool EstaAgendada => Status == StatusViagem.Agendada;

    public string ClasseStatus => Status switch
    {
        StatusViagem.Agendada => "bg-primary",
        StatusViagem.EmAndamento => "bg-warning text-dark",
        StatusViagem.Finalizada => "bg-success",
        _ => "bg-secondary"
    };

    public ViagemViewModel() { }

    public ViagemViewModel(Viagem viagem)
    {
        Id = viagem.Id;
        RotaId = viagem.RotaId;
        MotoristaId = viagem.MotoristaId;
        VeiculoId = viagem.VeiculoId;
        DataHoraPartida = viagem.DataHoraPartida;
        Tipo = viagem.Tipo;
        Status = viagem.Status;
    }
}