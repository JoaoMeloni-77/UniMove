using global::Dapper;
using Microsoft.Data.Sqlite;
using UniMove.Exceptions;
using UniMove.Models;
using UniMove.Models.ViewModels;
using UniMove.Repositories.Interfaces;

namespace UniMove.Repositories.Dapper;

public class ReservaRepository : IReservaRepository
{
    private const string Formato = "yyyy-MM-dd HH:mm:ss";
    private const string Colunas = "Id, ViagemId, AlunoId, PontoEmbarqueId, Status, CriadaEm";

    private readonly IConexaoFactory _fabrica;

    public ReservaRepository(IConexaoFactory fabrica)
    {
        _fabrica = fabrica;
    }

    public IEnumerable<Reserva> ObterTodos()
    {
        using var conexao = _fabrica.CriarConexao();
        return conexao.Query<Reserva>($"SELECT {Colunas} FROM Reservas ORDER BY Id").ToList();
    }

    public Reserva? ObterPorId(int id)
    {
        using var conexao = _fabrica.CriarConexao();
        return conexao.QueryFirstOrDefault<Reserva>(
            $"SELECT {Colunas} FROM Reservas WHERE Id = @Id", new { Id = id });
    }

    public int Adicionar(Reserva entidade)
    {
        using var conexao = _fabrica.CriarConexao();
        try
        {
            return conexao.ExecuteScalar<int>(
                @"INSERT INTO Reservas (ViagemId, AlunoId, PontoEmbarqueId, Status, CriadaEm)
                  VALUES (@ViagemId, @AlunoId, @PontoEmbarqueId, @Status, @CriadaEm);
                  SELECT last_insert_rowid();",
                new
                {
                    entidade.ViagemId,
                    entidade.AlunoId,
                    entidade.PontoEmbarqueId,
                    Status = entidade.Status.ToString(),
                    CriadaEm = entidade.CriadaEm.ToString(Formato)
                });
        }
        // 19 = violação de restrição (índice único parcial). Qualquer outro erro segue sem ser capturado.
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            throw new RegraNegocioException("Você já possui uma reserva ativa para esta viagem.");
        }
    }

    public void Atualizar(Reserva entidade)
    {
        using var conexao = _fabrica.CriarConexao();
        var linhas = conexao.Execute(
            "UPDATE Reservas SET Status = @Status WHERE Id = @Id",
            new { Status = entidade.Status.ToString(), entidade.Id });

        if (linhas == 0)
            throw new RegraNegocioException("Reserva não encontrada.");
    }

    public void Remover(int id)
    {
        throw new RegraNegocioException("Reservas não são excluídas: use o cancelamento.");
    }

    public ViagemParaReserva? ObterDadosViagem(int viagemId)
    {
        using var conexao = _fabrica.CriarConexao();
        return conexao.QueryFirstOrDefault<ViagemParaReserva>(
            @"SELECT v.Id, v.RotaId, v.DataHoraPartida, v.Status, ve.Capacidade,
                     (SELECT COUNT(*) FROM Reservas r
                       WHERE r.ViagemId = v.Id AND r.Status <> 'Cancelada') AS ReservasAtivas
              FROM Viagens v
              INNER JOIN Veiculos ve ON ve.Id = v.VeiculoId
              WHERE v.Id = @Id",
            new { Id = viagemId });
    }

    public IEnumerable<ViagemDisponivelViewModel> ObterViagensDisponiveis(int alunoId)
    {
        using var conexao = _fabrica.CriarConexao();
        return conexao.Query<ViagemDisponivelViewModel>(
            @"SELECT v.Id AS ViagemId, v.DataHoraPartida, v.Tipo, rt.Nome AS Rota, ve.Capacidade,
                     (SELECT COUNT(*) FROM Reservas x
                       WHERE x.ViagemId = v.Id AND x.Status <> 'Cancelada') AS ReservasAtivas,
                     (SELECT COUNT(*) FROM Reservas y
                       WHERE y.ViagemId = v.Id AND y.AlunoId = @AlunoId AND y.Status <> 'Cancelada') AS JaReservou
              FROM Viagens v
              INNER JOIN Rotas rt ON rt.Id = v.RotaId
              INNER JOIN Veiculos ve ON ve.Id = v.VeiculoId
              WHERE v.Status = 'Agendada' AND v.DataHoraPartida > @Agora
              ORDER BY v.DataHoraPartida",
            new { AlunoId = alunoId, Agora = DateTime.Now.ToString(Formato) }).ToList();
    }

    public IEnumerable<MinhaReservaViewModel> ObterReservasDoAluno(int alunoId)
    {
        using var conexao = _fabrica.CriarConexao();
        return conexao.Query<MinhaReservaViewModel>(
            @"SELECT rs.Id AS ReservaId, rs.ViagemId, v.DataHoraPartida, v.Tipo,
                     v.Status AS StatusViagem, rt.Nome AS Rota, p.Nome AS Ponto,
                     rs.Status AS StatusReserva
              FROM Reservas rs
              INNER JOIN Viagens v ON v.Id = rs.ViagemId
              INNER JOIN Rotas rt ON rt.Id = v.RotaId
              INNER JOIN PontosEmbarque p ON p.Id = rs.PontoEmbarqueId
              WHERE rs.AlunoId = @AlunoId
              ORDER BY v.DataHoraPartida DESC",
            new { AlunoId = alunoId }).ToList();
    }

    public IEnumerable<PassageiroViewModel> ObterPassageirosDaViagem(int viagemId)
    {
        using var conexao = _fabrica.CriarConexao();
        return conexao.Query<PassageiroViewModel>(
            @"SELECT rs.Id AS ReservaId, u.Nome AS AlunoNome, a.Matricula, p.Nome AS Ponto, rs.Status
              FROM Reservas rs
              INNER JOIN Usuarios u ON u.Id = rs.AlunoId
              INNER JOIN Alunos a ON a.UsuarioId = rs.AlunoId
              INNER JOIN PontosEmbarque p ON p.Id = rs.PontoEmbarqueId
              WHERE rs.ViagemId = @ViagemId AND rs.Status <> 'Cancelada'
              ORDER BY p.Ordem, u.Nome",
            new { ViagemId = viagemId }).ToList();
    }

    public IEnumerable<PontoOpcao> ObterPontosDaViagem(int viagemId)
    {
        using var conexao = _fabrica.CriarConexao();
        return conexao.Query<PontoOpcao>(
            @"SELECT p.Id, p.Nome
              FROM PontosEmbarque p
              INNER JOIN Viagens v ON v.RotaId = p.RotaId
              WHERE v.Id = @ViagemId
              ORDER BY p.Ordem",
            new { ViagemId = viagemId }).ToList();
    }

    public bool PontoPertenceARota(int pontoId, int rotaId)
    {
        using var conexao = _fabrica.CriarConexao();
        return conexao.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM PontosEmbarque WHERE Id = @PontoId AND RotaId = @RotaId",
            new { PontoId = pontoId, RotaId = rotaId }) > 0;
    }

    public bool AlunoJaReservou(int alunoId, int viagemId)
    {
        using var conexao = _fabrica.CriarConexao();
        return conexao.ExecuteScalar<int>(
            @"SELECT COUNT(*) FROM Reservas
              WHERE AlunoId = @AlunoId AND ViagemId = @ViagemId AND Status <> 'Cancelada'",
            new { AlunoId = alunoId, ViagemId = viagemId }) > 0;
    }
}