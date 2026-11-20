using global::Dapper;
using UniMove.Exceptions;
using UniMove.Models;
using UniMove.Repositories.Interfaces;

namespace UniMove.Repositories.Dapper;

public class MotoristaRepository : IMotoristaRepository
{
    private const string SelectBase =
        @"SELECT u.Id, u.Nome, u.Email, u.SenhaHash, u.Ativo, m.Cnh, m.Telefone
          FROM Usuarios u
          INNER JOIN Motoristas m ON m.UsuarioId = u.Id";

    private readonly IConexaoFactory _fabrica;

    public MotoristaRepository(IConexaoFactory fabrica)
    {
        _fabrica = fabrica;
    }

    public IEnumerable<Motorista> ObterTodos()
    {
        using var conexao = _fabrica.CriarConexao();
        return conexao.Query<Motorista>(SelectBase + " ORDER BY u.Nome").ToList();
    }

    public Motorista? ObterPorId(int id)
    {
        using var conexao = _fabrica.CriarConexao();
        return conexao.QueryFirstOrDefault<Motorista>(
            SelectBase + " WHERE u.Id = @Id",
            new { Id = id });
    }

    public int Adicionar(Motorista entidade)
    {
        ValidarDuplicidade(entidade.Email, entidade.Cnh, 0);

        using var conexao = _fabrica.CriarConexao();
        using var transacao = conexao.BeginTransaction();
        try
        {
            var id = conexao.ExecuteScalar<int>(
                @"INSERT INTO Usuarios (Nome, Email, SenhaHash, Perfil, Ativo)
                  VALUES (@Nome, @Email, @SenhaHash, @Perfil, @Ativo);
                  SELECT last_insert_rowid();",
                new
                {
                    entidade.Nome,
                    entidade.Email,
                    entidade.SenhaHash,
                    Perfil = entidade.Perfil.ToString(),
                    Ativo = entidade.Ativo ? 1 : 0
                },
                transacao);

            conexao.Execute(
                @"INSERT INTO Motoristas (UsuarioId, Cnh, Telefone)
                  VALUES (@Id, @Cnh, @Telefone)",
                new { Id = id, entidade.Cnh, entidade.Telefone },
                transacao);

            transacao.Commit();
            return id;
        }
        catch
        {
            transacao.Rollback();
            throw;
        }
    }

    public void Atualizar(Motorista entidade)
    {
        ValidarDuplicidade(entidade.Email, entidade.Cnh, entidade.Id);

        using var conexao = _fabrica.CriarConexao();
        using var transacao = conexao.BeginTransaction();
        try
        {
            var linhas = conexao.Execute(
                @"UPDATE Usuarios SET Nome = @Nome, Email = @Email, Ativo = @Ativo
                  WHERE Id = @Id",
                new
                {
                    entidade.Nome,
                    entidade.Email,
                    Ativo = entidade.Ativo ? 1 : 0,
                    entidade.Id
                },
                transacao);

            if (linhas == 0)
                throw new RegraNegocioException("Motorista não encontrado.");

            conexao.Execute(
                @"UPDATE Motoristas SET Cnh = @Cnh, Telefone = @Telefone
                  WHERE UsuarioId = @Id",
                new { entidade.Cnh, entidade.Telefone, entidade.Id },
                transacao);

            transacao.Commit();
        }
        catch
        {
            transacao.Rollback();
            throw;
        }
    }

    public void Remover(int id)
    {
        if (PossuiViagens(id))
            throw new RegraNegocioException(
                "Não é possível excluir: o motorista possui viagens vinculadas.");

        using var conexao = _fabrica.CriarConexao();
        using var transacao = conexao.BeginTransaction();
        try
        {
            conexao.Execute(
                "DELETE FROM Motoristas WHERE UsuarioId = @Id",
                new { Id = id }, transacao);

            var linhas = conexao.Execute(
                "DELETE FROM Usuarios WHERE Id = @Id",
                new { Id = id }, transacao);

            if (linhas == 0)
                throw new RegraNegocioException("Motorista não encontrado.");

            transacao.Commit();
        }
        catch
        {
            transacao.Rollback();
            throw;
        }
    }

    public bool PossuiViagens(int motoristaId)
    {
        using var conexao = _fabrica.CriarConexao();
        return conexao.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM Viagens WHERE MotoristaId = @Id",
            new { Id = motoristaId }) > 0;
    }

    public bool EmailJaExiste(string email, int ignorarId = 0)
    {
        using var conexao = _fabrica.CriarConexao();
        return conexao.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM Usuarios WHERE LOWER(Email) = LOWER(@Email) AND Id <> @Id",
            new { Email = email, Id = ignorarId }) > 0;
    }

    public bool CnhJaExiste(string cnh, int ignorarId = 0)
    {
        using var conexao = _fabrica.CriarConexao();
        return conexao.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM Motoristas WHERE Cnh = @Cnh AND UsuarioId <> @Id",
            new { Cnh = cnh, Id = ignorarId }) > 0;
    }

    private void ValidarDuplicidade(string email, string cnh, int ignorarId)
    {
        if (EmailJaExiste(email, ignorarId))
            throw new RegraNegocioException("Já existe um usuário com este e-mail.");

        if (CnhJaExiste(cnh, ignorarId))
            throw new RegraNegocioException("Já existe um motorista com esta CNH.");
    }
}