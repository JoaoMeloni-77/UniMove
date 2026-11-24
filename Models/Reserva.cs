using UniMove.Exceptions;
using UniMove.Models.Enums;

namespace UniMove.Models;

public class Reserva
{
    public int Id { get; private set; }
    public int ViagemId { get; private set; }
    public int AlunoId { get; private set; }
    public int PontoEmbarqueId { get; private set; }
    public StatusReserva Status { get; private set; }
    public DateTime CriadaEm { get; private set; }

    // Reserva cancelada não ocupa vaga
    public bool OcupaVaga => Status != StatusReserva.Cancelada;

    public Reserva(int viagemId, int alunoId, int pontoEmbarqueId)
    {
        if (viagemId <= 0) throw new RegraNegocioException("A reserva deve estar ligada a uma viagem.");
        if (alunoId <= 0) throw new RegraNegocioException("A reserva deve estar ligada a um aluno.");
        if (pontoEmbarqueId <= 0) throw new RegraNegocioException("Escolha um ponto de embarque.");

        ViagemId = viagemId;
        AlunoId = alunoId;
        PontoEmbarqueId = pontoEmbarqueId;
        Status = StatusReserva.Confirmada;
        CriadaEm = DateTime.Now;
    }

    // Usado pelo Dapper
    private Reserva() { }

    public void Cancelar()
    {
        if (Status != StatusReserva.Confirmada)
            throw new RegraNegocioException("Só é possível cancelar reservas confirmadas.");
        Status = StatusReserva.Cancelada;
    }

    public void MarcarEmbarcado()
    {
        if (Status != StatusReserva.Confirmada)
            throw new RegraNegocioException("Só é possível confirmar embarque de reservas confirmadas.");
        Status = StatusReserva.Embarcado;
    }

    public void MarcarAusente()
    {
        if (Status != StatusReserva.Confirmada)
            throw new RegraNegocioException("Só é possível marcar ausência de reservas confirmadas.");
        Status = StatusReserva.Ausente;
    }
}