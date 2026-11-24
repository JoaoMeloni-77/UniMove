using global::Dapper;
using UniMove.Exceptions;
using UniMove.Models;
using UniMove.Repositories.Interfaces;

namespace UniMove.Repositories.Dapper;

public class AlunoRepository : IAlunoRepository
{
    private const string SelectBase =
        @"SELECT u.Id, u.Nome, u.Email, u.SenhaHash, u.Ativo,
                 a.InstituicaoId, a.Matricula, a.Curso
          FROM Usuarios u
          INNER JOIN Alunos a ON a.UsuarioId = u.Id";

    private readonly IConexaoFactory _fabrica;

    public AlunoRepository(IConexaoFactory fabrica)
    {
        _fabrica = fabrica;
    }

    public IEnumerable<Aluno> ObterTodos()
    {
        using var conexao = _fabrica.CriarConexao();
        return conexao.Query<Aluno>(SelectBase + " ORDER BY u.Nome").ToList();
    }

    public Aluno? ObterPorId(int id)
    {
        using var conexao = _fabrica.CriarConexao();
        return conexao.QueryFirstOrDefault<Aluno>(
            SelectBase + " WHERE u.Id = @Id",
            new { Id = id });
    }

    public int Adicionar(Aluno entidade)
    {
        ValidarRegras(entidade, 0);

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
                @"INSERT INTO Alunos (UsuarioId, InstituicaoId, Matricula, Curso)
                  VALUES (@Id, @InstituicaoId, @Matricula, @Curso)",
                new { Id = id, entidade.InstituicaoId, entidade.Matricula, entidade.Curso },
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

    public void Atualizar(Aluno entidade)
    {
        ValidarRegras(entidade, entidade.Id);

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
                throw new RegraNegocioException("Aluno não encontrado.");

            conexao.Execute(
                @"UPDATE Alunos SET InstituicaoId = @InstituicaoId, Matricula = @Matricula, Curso = @Curso
                  WHERE UsuarioId = @Id",
                new { entidade.InstituicaoId, entidade.Matricula, entidade.Curso, entidade.Id },
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
        if (PossuiReservas(id))
            throw new RegraNegocioException(
                "Não é possível excluir: o aluno possui reservas vinculadas.");

        using var conexao = _fabrica.CriarConexao();
        using var transacao = conexao.BeginTransaction();
        try
        {
            conexao.Execute(
                "DELETE FROM Alunos WHERE UsuarioId = @Id",
                new { Id = id }, transacao);

            var linhas = conexao.Execute(
                "DELETE FROM Usuarios WHERE Id = @Id",
                new { Id = id }, transacao);

            if (linhas == 0)
                throw new RegraNegocioException("Aluno não encontrado.");

            transacao.Commit();
        }
        catch
        {
            transacao.Rollback();
            throw;
        }
    }

    public bool PossuiReservas(int alunoId)
    {
        using var conexao = _fabrica.CriarConexao();
        return conexao.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM Reservas WHERE AlunoId = @Id",
            new { Id = alunoId }) > 0;
    }

    public bool EmailJaExiste(string email, int ignorarId = 0)
    {
        using var conexao = _fabrica.CriarConexao();
        return conexao.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM Usuarios WHERE LOWER(Email) = LOWER(@Email) AND Id <> @Id",
            new { Email = email, Id = ignorarId }) > 0;
    }

    public bool MatriculaJaExiste(string matricula, int ignorarId = 0)
    {
        using var conexao = _fabrica.CriarConexao();
        return conexao.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM Alunos WHERE LOWER(Matricula) = LOWER(@Matricula) AND UsuarioId <> @Id",
            new { Matricula = matricula, Id = ignorarId }) > 0;
    }

    private bool InstituicaoExiste(int instituicaoId)
    {
        using var conexao = _fabrica.CriarConexao();
        return conexao.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM Instituicoes WHERE Id = @Id",
            new { Id = instituicaoId }) > 0;
    }

    private void ValidarRegras(Aluno aluno, int ignorarId)
    {
        if (!InstituicaoExiste(aluno.InstituicaoId))
            throw new RegraNegocioException("A instituição informada não existe.");

        if (EmailJaExiste(aluno.Email, ignorarId))
            throw new RegraNegocioException("Já existe um usuário com este e-mail.");

        if (MatriculaJaExiste(aluno.Matricula, ignorarId))
            throw new RegraNegocioException("Já existe um aluno com esta matrícula.");
    }
}