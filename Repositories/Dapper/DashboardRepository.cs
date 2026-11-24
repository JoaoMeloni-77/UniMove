using global::Dapper;
using UniMove.Models.ViewModels;
using UniMove.Repositories.Interfaces;

namespace UniMove.Repositories.Dapper;

public class DashboardRepository : IDashboardRepository
{
    private readonly IConexaoFactory _fabrica;

    public DashboardRepository(IConexaoFactory fabrica)
    {
        _fabrica = fabrica;
    }

    public IndicadoresViewModel ObterIndicadores()
    {
        using var conexao = _fabrica.CriarConexao();
        return conexao.QuerySingle<IndicadoresViewModel>(
            @"SELECT
                (SELECT COUNT(*) FROM Viagens WHERE Status <> 'Cancelada') AS TotalViagens,
                (SELECT COUNT(*) FROM Alunos) AS TotalAlunos,
                (SELECT COUNT(*) FROM Reservas rs
                   INNER JOIN Viagens v ON v.Id = rs.ViagemId
                  WHERE rs.Status <> 'Cancelada'
                    AND v.Status IN ('Agendada', 'EmAndamento')) AS PassageirosPrevistos,
                (SELECT COALESCE(SUM(ve.Capacidade), 0) FROM Viagens v
                   INNER JOIN Veiculos ve ON ve.Id = v.VeiculoId
                  WHERE v.Status IN ('Agendada', 'EmAndamento')) AS CapacidadePrevista");
    }

    public IEnumerable<OcupacaoViagemItem> ObterOcupacaoPorViagem()
    {
        using var conexao = _fabrica.CriarConexao();
        return conexao.Query<OcupacaoViagemItem>(
            @"SELECT v.DataHoraPartida, rt.Nome AS Rota, ve.Capacidade,
                     (SELECT COUNT(*) FROM Reservas r
                       WHERE r.ViagemId = v.Id AND r.Status <> 'Cancelada') AS Reservas
              FROM Viagens v
              INNER JOIN Rotas rt ON rt.Id = v.RotaId
              INNER JOIN Veiculos ve ON ve.Id = v.VeiculoId
              WHERE v.Status IN ('Agendada', 'EmAndamento')
              ORDER BY v.DataHoraPartida
              LIMIT 10").ToList();
    }

    public IEnumerable<RelatorioLinha> ObterRelatorio(
        int instituicaoId, DateTime? inicio, DateTime? fim, int horaMin, int horaMax)
    {
        using var conexao = _fabrica.CriarConexao();
        return conexao.Query<RelatorioLinha>(
            @"SELECT v.Id AS ViagemId, v.DataHoraPartida, v.Tipo, rt.Nome AS Rota,
                     i.Nome AS Instituicao, COUNT(*) AS Passageiros
              FROM Reservas rs
              INNER JOIN Viagens v ON v.Id = rs.ViagemId
              INNER JOIN Rotas rt ON rt.Id = v.RotaId
              INNER JOIN Alunos a ON a.UsuarioId = rs.AlunoId
              INNER JOIN Instituicoes i ON i.Id = a.InstituicaoId
              WHERE rs.Status <> 'Cancelada'
                AND (@InstituicaoId = 0 OR i.Id = @InstituicaoId)
                AND (@Inicio IS NULL OR substr(v.DataHoraPartida, 1, 10) >= @Inicio)
                AND (@Fim IS NULL OR substr(v.DataHoraPartida, 1, 10) <= @Fim)
                AND CAST(substr(v.DataHoraPartida, 12, 2) AS INTEGER) BETWEEN @HoraMin AND @HoraMax
              GROUP BY v.Id, i.Id
              ORDER BY v.DataHoraPartida, i.Nome",
            new
            {
                InstituicaoId = instituicaoId,
                Inicio = inicio?.ToString("yyyy-MM-dd"),
                Fim = fim?.ToString("yyyy-MM-dd"),
                HoraMin = horaMin,
                HoraMax = horaMax
            }).ToList();
    }
}