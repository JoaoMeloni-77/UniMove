using global::Dapper;
using UniMove.Exceptions;
using UniMove.Models;
using UniMove.Repositories.Interfaces;

namespace UniMove.Repositories.Dapper;

public class InstituicaoRepository : IInstituicaoRepository
{
    private readonly IConexaoFactory _fabrica;

    public InstituicaoRepository(IConexaoFactory fabrica)
    {
        _fabrica = fabrica;
    }

    public IEnumerable<Instituicao> ObterTodos()
    {
        using var conexao = _fabrica.CriarConexao();
        return conexao.Query<Instituicao>(
            "SELECT Id, Nome, Cidade FROM Instituicoes ORDER BY Nome").ToList();
    }

    public Instituicao? ObterPorId(int id)
    {
        using var conexao = _fabrica.CriarConexao();
        return conexao.QueryFirstOrDefault<Instituicao>(
            "SELECT Id, Nome, Cidade FROM Instituicoes WHERE Id = @Id",
            new { Id = id });
    }

    public int Adicionar(Instituicao entidade)
    {
        using var conexao = _fabrica.CriarConexao();
        return conexao.ExecuteScalar<int>(
            @"INSERT INTO Instituicoes (Nome, Cidade)
              VALUES (@Nome, @Cidade);
              SELECT last_insert_rowid();",
            new { entidade.Nome, entidade.Cidade });
    }

    public void Atualizar(Instituicao entidade)
    {
        using var conexao = _fabrica.CriarConexao();
        var linhas = conexao.Execute(
            "UPDATE Instituicoes SET Nome = @Nome, Cidade = @Cidade WHERE Id = @Id",
            new { entidade.Nome, entidade.Cidade, entidade.Id });

        if (linhas == 0)
            throw new RegraNegocioException("Instituição não encontrada.");
    }

    public void Remover(int id)
    {
        if (PossuiAlunos(id))
            throw new RegraNegocioException(
                "Não é possível excluir: existem alunos vinculados a esta instituição.");

        using var conexao = _fabrica.CriarConexao();
        var linhas = conexao.Execute(
            "DELETE FROM Instituicoes WHERE Id = @Id",
            new { Id = id });

        if (linhas == 0)
            throw new RegraNegocioException("Instituição não encontrada.");
    }

    public bool PossuiAlunos(int instituicaoId)
    {
        using var conexao = _fabrica.CriarConexao();
        var total = conexao.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM Alunos WHERE InstituicaoId = @Id",
            new { Id = instituicaoId });
        return total > 0;
    }
}