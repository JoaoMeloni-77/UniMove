using UniMove.Exceptions;
using UniMove.Models.Enums;
using UniMove.Repositories.Interfaces;

namespace UniMove.Services;

public class OperacaoViagemService
{
    private readonly IViagemRepository _viagens;
    private readonly IReservaRepository _reservas;

    public OperacaoViagemService(IViagemRepository viagens, IReservaRepository reservas)
    {
        _viagens = viagens;
        _reservas = reservas;
    }

    public void Iniciar(int viagemId, int motoristaId)
    {
        var viagem = _viagens.ObterPorId(viagemId)
            ?? throw new RegraNegocioException("Viagem não encontrada.");

        viagem.Iniciar(motoristaId);          // a regra de status vive na entidade
        _viagens.Atualizar(viagem);
    }

    public void Finalizar(int viagemId, int motoristaId)
    {
        var viagem = _viagens.ObterPorId(viagemId)
            ?? throw new RegraNegocioException("Viagem não encontrada.");

        viagem.Finalizar(motoristaId);
        _viagens.Atualizar(viagem);
    }

    public void ConfirmarEmbarque(int reservaId, int motoristaId)
    {
        var reserva = _reservas.ObterPorId(reservaId)
            ?? throw new RegraNegocioException("Reserva não encontrada.");

        var viagem = _viagens.ObterPorId(reserva.ViagemId)
            ?? throw new RegraNegocioException("Viagem não encontrada.");

        if (viagem.MotoristaId != motoristaId)
            throw new RegraNegocioException("Somente o motorista da viagem pode confirmar embarques.");

        if (viagem.Status != StatusViagem.EmAndamento)
            throw new RegraNegocioException("Só é possível confirmar embarque em viagens em andamento.");

        reserva.MarcarEmbarcado();            // exige reserva Confirmada
        _reservas.Atualizar(reserva);
    }
}