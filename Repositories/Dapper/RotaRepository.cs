using System.Data;
using global::Dapper;
using UniMove.Exceptions;
using UniMove.Models;
using UniMove.Repositories.Interfaces;

namespace UniMove.Repositories.Dapper;

public class RotaRepository : IRotaRepository
{
    private readonly IConexaoFactory _fabrica;

    public RotaRepository(IConexaoFactory fabrica)
    {
        _fabrica = fabrica;
    }

    public IEnumerable<Rota> ObterTodos()
    {
        using var conexao = _fabrica.CriarConexao();

        var rotas = conexao.Query<Rota>(
            "SELECT Id, Nome, DuracaoMinutos FROM Rotas ORDER BY Nome").ToList();

        var pontos = conexao.Query<PontoEmbarque>(
            "SELECT Id, RotaId, Nome, Referencia, Ordem FROM PontosEmbarque ORDER BY RotaId, Ordem").ToList();

        foreach (var rota in rotas)
            rota.CarregarPontos(pontos.Where(p => p.RotaId == rota.Id));

        return rotas;
    }

    public Rota? ObterPorId(int id)
    {
        using var conexao = _fabrica.CriarConexao();

        var rota = conexao.QueryFirstOrDefault<Rota>(
            "SELECT Id, Nome, DuracaoMinutos FROM Rotas WHERE Id = @Id",
            new { Id = id });

        if (rota == null)
            return null;

        var pontos = conexao.Query<PontoEmbarque>(
            "SELECT Id, RotaId, Nome, Referencia, Ordem FROM PontosEmbarque WHERE RotaId = @Id ORDER BY Ordem",
            new { Id = id });

        rota.CarregarPontos(pontos);
        return rota;
    }

    public int Adicionar(Rota entidade)
    {
        using var conexao = _fabrica.CriarConexao();
        using var transacao = conexao.BeginTransaction();
        try
        {
            var id = conexao.ExecuteScalar<int>(
                @"INSERT INTO Rotas (Nome, DuracaoMinutos)
                  VALUES (@Nome, @DuracaoMinutos);
                  SELECT last_insert_rowid();",
                new { entidade.Nome, entidade.DuracaoMinutos },
                transacao);

            SalvarPontos(conexao, transacao, id, entidade.Pontos);

            transacao.Commit();
            return id;
        }
        catch
        {
            transacao.Rollback();
            throw;
        }
    }

    public void Atualizar(Rota entidade)
    {
        using var conexao = _fabrica.CriarConexao();
        using var transacao = conexao.BeginTransaction();
        try
        {
            var linhas = conexao.Execute(
                "UPDATE Rotas SET Nome = @Nome, DuracaoMinutos = @DuracaoMinutos WHERE Id = @Id",
                new { entidade.Nome, entidade.DuracaoMinutos, entidade.Id },
                transacao);

            if (linhas == 0)
                throw new RegraNegocioException("Rota não encontrada.");

            // Pontos que estão no banco mas saíram da rota
            var idsNoBanco = conexao.Query<int>(
                "SELECT Id FROM PontosEmbarque WHERE RotaId = @Id",
                new { entidade.Id },
                transacao).ToList();

            var idsAtuais = entidade.Pontos.Select(p => p.Id).ToHashSet();

            foreach (var idRemovido in idsNoBanco.Where(i => !idsAtuais.Contains(i)))
            {
                var reservas = conexao.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Reservas WHERE PontoEmbarqueId = @Id",
                    new { Id = idRemovido },
                    transacao);

                if (reservas > 0)
                    throw new RegraNegocioException(
                        "Não é possível remover o ponto: existem reservas que usam este ponto de embarque.");

                conexao.Execute(
                    "DELETE FROM PontosEmbarque WHERE Id = @Id",
                    new { Id = idRemovido },
                    transacao);
            }

            SalvarPontos(conexao, transacao, entidade.Id, entidade.Pontos);

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
                "Não é possível excluir: existem viagens vinculadas a esta rota.");

        using var conexao = _fabrica.CriarConexao();
        using var transacao = conexao.BeginTransaction();
        try
        {
            // Os pontos primeiro, por causa da chave estrangeira
            conexao.Execute(
                "DELETE FROM PontosEmbarque WHERE RotaId = @Id",
                new { Id = id }, transacao);

            var linhas = conexao.Execute(
                "DELETE FROM Rotas WHERE Id = @Id",
                new { Id = id }, transacao);

            if (linhas == 0)
                throw new RegraNegocioException("Rota não encontrada.");

            transacao.Commit();
        }
        catch
        {
            transacao.Rollback();
            throw;
        }
    }

    public bool PossuiViagens(int rotaId)
    {
        using var conexao = _fabrica.CriarConexao();
        return conexao.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM Viagens WHERE RotaId = @Id",
            new { Id = rotaId }) > 0;
    }

    // Ponto com Id 0 é novo (INSERT); os demais já existem (UPDATE)
    private static void SalvarPontos(
        IDbConnection conexao, IDbTransaction transacao, int rotaId, IEnumerable<PontoEmbarque> pontos)
    {
        foreach (var ponto in pontos)
        {
            if (ponto.Id == 0)
            {
                conexao.Execute(
                    @"INSERT INTO PontosEmbarque (RotaId, Nome, Referencia, Ordem)
                      VALUES (@RotaId, @Nome, @Referencia, @Ordem)",
                    new { RotaId = rotaId, ponto.Nome, ponto.Referencia, ponto.Ordem },
                    transacao);
            }
            else
            {
                conexao.Execute(
                    @"UPDATE PontosEmbarque
                      SET Nome = @Nome, Referencia = @Referencia, Ordem = @Ordem
                      WHERE Id = @Id",
                    new { ponto.Nome, ponto.Referencia, ponto.Ordem, ponto.Id },
                    transacao);
            }
        }
    }
}