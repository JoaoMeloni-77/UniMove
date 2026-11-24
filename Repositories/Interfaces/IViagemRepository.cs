using UniMove.Models;

namespace UniMove.Repositories.Interfaces;

public interface IViagemRepository : IRepository<Viagem>
{
    bool PossuiReservas(int viagemId);
    int ContarReservasAtivas(int viagemId);
    bool MotoristaOcupado(int motoristaId, DateTime inicio, DateTime fim, int ignorarViagemId = 0);
    bool VeiculoOcupado(int veiculoId, DateTime inicio, DateTime fim, int ignorarViagemId = 0);
}