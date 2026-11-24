using UniMove.Exceptions;
using UniMove.Models;
using UniMove.Repositories.Interfaces;

namespace UniMove.Services;

public class ViagemService : IViagemService
{
    private readonly IViagemRepository _viagens;
    private readonly IRotaRepository _rotas;
    private readonly IMotoristaRepository _motoristas;
    private readonly IVeiculoRepository _veiculos;

    public ViagemService(
        IViagemRepository viagens,
        IRotaRepository rotas,
        IMotoristaRepository motoristas,
        IVeiculoRepository veiculos)
    {
        _viagens = viagens;
        _rotas = rotas;
        _motoristas = motoristas;
        _veiculos = veiculos;
    }

    public int Cadastrar(Viagem viagem)
    {
        ValidarReferenciasEConflitos(viagem);
        return _viagens.Adicionar(viagem);
    }

    public void Alterar(Viagem viagem)
    {
        var existente = _viagens.ObterPorId(viagem.Id)
            ?? throw new RegraNegocioException("Viagem não encontrada.");

        // As reservas apontam para pontos da rota antiga
        if (existente.RotaId != viagem.RotaId && _viagens.PossuiReservas(viagem.Id))
            throw new RegraNegocioException(
                "Não é possível trocar a rota: a viagem já possui reservas.");

        ValidarReferenciasEConflitos(viagem);
        _viagens.Atualizar(viagem);
    }

    private void ValidarReferenciasEConflitos(Viagem viagem)
    {
        var rota = _rotas.ObterPorId(viagem.RotaId)
            ?? throw new RegraNegocioException("Rota não encontrada.");

        if (_motoristas.ObterPorId(viagem.MotoristaId) == null)
            throw new RegraNegocioException("Motorista não encontrado.");

        var veiculo = _veiculos.ObterPorId(viagem.VeiculoId)
            ?? throw new RegraNegocioException("Veículo não encontrado.");

        // Ao trocar o veículo, ele precisa comportar as reservas que já existem
        if (viagem.Id != 0)
        {
            var ativas = _viagens.ContarReservasAtivas(viagem.Id);
            if (ativas > veiculo.Capacidade)
                throw new RegraNegocioException(
                    $"O veículo escolhido tem {veiculo.Capacidade} lugares, mas a viagem já tem {ativas} reservas ativas.");
        }

        var inicio = viagem.DataHoraPartida;
        var fim = viagem.CalcularChegadaPrevista(rota.DuracaoMinutos);

        if (_viagens.MotoristaOcupado(viagem.MotoristaId, inicio, fim, viagem.Id))
            throw new RegraNegocioException("O motorista já possui outra viagem neste horário.");

        if (_viagens.VeiculoOcupado(viagem.VeiculoId, inicio, fim, viagem.Id))
            throw new RegraNegocioException("O veículo já está em outra viagem neste horário.");
    }
}