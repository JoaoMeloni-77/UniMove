using UniMove.Models;

namespace UniMove.Repositories.Interfaces;

public interface IMotoristaRepository : IRepository<Motorista>
{
    bool PossuiViagens(int motoristaId);
    bool EmailJaExiste(string email, int ignorarId = 0);
    bool CnhJaExiste(string cnh, int ignorarId = 0);
}