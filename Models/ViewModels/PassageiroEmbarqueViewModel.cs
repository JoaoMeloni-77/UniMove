using UniMove.Models.Enums;

namespace UniMove.Models.ViewModels;

public class PassageiroEmbarqueViewModel
{
    public int ReservaId { get; set; }
    public string NomeAluno { get; set; } = string.Empty;
    public string PontoNome { get; set; } = string.Empty;
    public StatusReserva Status { get; set; }
}