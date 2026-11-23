using UniMove.Models;

namespace UniMove.Repositories.Interfaces;

public interface IVeiculoRepository : IRepository<Veiculo>
{
    bool PossuiViagens(int veiculoId);
}