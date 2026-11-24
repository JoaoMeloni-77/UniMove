using global::Dapper;
using UniMove.Models;
using UniMove.Repositories.Interfaces;

namespace UniMove.Repositories.Dapper;

public class UsuarioRepository : IUsuarioRepository
{
    private const string SelectAluno =
        @"SELECT u.Id, u.Nome, u.Email, u.SenhaHash, u.Ativo, a.InstituicaoId, a.Matricula, a.Curso
          FROM Usuarios u INNER JOIN Alunos a ON a.UsuarioId = u.Id
          WHERE LOWER(u.Email) = LOWER(@Email)";

    private const string SelectMotorista =
        @"SELECT u.Id, u.Nome, u.Email, u.SenhaHash, u.Ativo, m.Cnh, m.Telefone
          FROM Usuarios u INNER JOIN Motoristas m ON m.UsuarioId = u.Id
          WHERE LOWER(u.Email) = LOWER(@Email)";

    private const string SelectAdministrador =
        @"SELECT Id, Nome, Email, SenhaHash, Ativo
          FROM Usuarios WHERE LOWER(Email) = LOWER(@Email)";

    private readonly IConexaoFactory _fabrica;

    public UsuarioRepository(IConexaoFactory fabrica)
    {
        _fabrica = fabrica;
    }

    // Devolve o tipo base: a subclasse certa é escolhida pelo Perfil gravado no banco
    public Usuario? ObterPorEmail(string email)
    {
        using var conexao = _fabrica.CriarConexao();
        var parametros = new { Email = email };

        var perfil = conexao.QueryFirstOrDefault<string>(
            "SELECT Perfil FROM Usuarios WHERE LOWER(Email) = LOWER(@Email)", parametros);

        return perfil switch
        {
            "Aluno" => (Usuario?)conexao.QueryFirstOrDefault<Aluno>(SelectAluno, parametros),
            "Motorista" => conexao.QueryFirstOrDefault<Motorista>(SelectMotorista, parametros),
            "Administrador" => conexao.QueryFirstOrDefault<Administrador>(SelectAdministrador, parametros),
            _ => null
        };
    }

    public void AtualizarSenha(int usuarioId, string novoHash)
    {
        using var conexao = _fabrica.CriarConexao();
        conexao.Execute(
            "UPDATE Usuarios SET SenhaHash = @Hash WHERE Id = @Id",
            new { Hash = novoHash, Id = usuarioId });
    }
}