using UniMove.Exceptions;
using UniMove.Models;
using UniMove.Models.Enums;
using UniMove.Repositories.Interfaces;

namespace UniMove.Services;

public class ReservaService
{
    private readonly IReservaRepository _reservas;

    public ReservaService(IReservaRepository reservas)
    {
        _reservas = reservas;
    }

    public int Reservar(int alunoId, int viagemId, int pontoId)
    {
        var viagem = _reservas.ObterDadosViagem(viagemId)
            ?? throw new RegraNegocioException("Viagem não encontrada.");

        if (viagem.Status != StatusViagem.Agendada)
            throw new RegraNegocioException("Só é possível reservar em viagens agendadas.");

        if (viagem.DataHoraPartida <= DateTime.Now)
            throw new RegraNegocioException("Esta viagem já partiu.");

        if (!_reservas.PontoPertenceARota(pontoId, viagem.RotaId))
            throw new RegraNegocioException("O ponto de embarque não pertence à rota desta viagem.");

        if (_reservas.AlunoJaReservou(alunoId, viagemId))
            throw new RegraNegocioException("Você já possui uma reserva ativa para esta viagem.");

        if (viagem.VagasDisponiveis <= 0)
            throw new RegraNegocioException("Viagem lotada: não há vagas disponíveis.");

        var reserva = new Reserva(viagemId, alunoId, pontoId);
        return _reservas.Adicionar(reserva);
    }

    public void Cancelar(int reservaId, int alunoId)
    {
        var reserva = _reservas.ObterPorId(reservaId)
            ?? throw new RegraNegocioException("Reserva não encontrada.");

        if (reserva.AlunoId != alunoId)
            throw new RegraNegocioException("Você só pode cancelar as suas próprias reservas.");

        var viagem = _reservas.ObterDadosViagem(reserva.ViagemId)
            ?? throw new RegraNegocioException("Viagem não encontrada.");

        if (viagem.Status != StatusViagem.Agendada || viagem.DataHoraPartida <= DateTime.Now)
            throw new RegraNegocioException("Não é possível cancelar: a viagem já foi iniciada ou encerrada.");

        reserva.Cancelar();            // regra de estado vive na entidade
        _reservas.Atualizar(reserva);
    }
}