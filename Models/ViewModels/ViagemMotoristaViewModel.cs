using UniMove.Models.Enums;

namespace UniMove.Models.ViewModels;

public class ViagemMotoristaViewModel
{
    public int Id { get; set; }
    public int MotoristaId { get; set; }
    public DateTime DataHoraPartida { get; set; }
    public TipoViagem Tipo { get; set; }
    public StatusViagem Status { get; set; }
    public string RotaNome { get; set; } = string.Empty;
    public string VeiculoDescricao { get; set; } = string.Empty;
    public int Capacidade { get; set; }
    public int ReservasAtivas { get; set; }
    public List<PassageiroEmbarqueViewModel> Passageiros { get; set; } = new();

    public bool PodeIniciar => Status == StatusViagem.Agendada;
    public bool PodeFinalizar => Status == StatusViagem.EmAndamento;

    public string ClasseStatus => Status switch
    {
        StatusViagem.Agendada => "bg-primary",
        StatusViagem.EmAndamento => "bg-warning text-dark",
        StatusViagem.Finalizada => "bg-success",
        _ => "bg-secondary"
    };
}