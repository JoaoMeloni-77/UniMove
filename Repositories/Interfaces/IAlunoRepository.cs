using UniMove.Models;

namespace UniMove.Repositories.Interfaces;

public interface IAlunoRepository : IRepository<Aluno>
{
    bool PossuiReservas(int alunoId);
    bool EmailJaExiste(string email, int ignorarId = 0);
    bool MatriculaJaExiste(string matricula, int ignorarId = 0);
}