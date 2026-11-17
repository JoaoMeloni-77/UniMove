using UniMove.Models;

namespace UniMove.Repositories.Interfaces;

public interface IInstituicaoRepository : IRepository<Instituicao>
{
    bool PossuiAlunos(int instituicaoId);
}