using UniMove.Models;

namespace UniMove.Repositories.Interfaces;

public interface IRotaRepository : IRepository<Rota>
{
    bool PossuiViagens(int rotaId);
}