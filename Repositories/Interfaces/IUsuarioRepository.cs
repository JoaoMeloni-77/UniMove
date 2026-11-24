using UniMove.Models;

namespace UniMove.Repositories.Interfaces;

public interface IUsuarioRepository
{
    Usuario? ObterPorEmail(string email);
    void AtualizarSenha(int usuarioId, string novoHash);
}