using System.Globalization;
using global::Dapper;
using UniMove.Exceptions;
using UniMove.Models;
using UniMove.Repositories.Interfaces;

namespace UniMove.Repositories.Dapper;

public class ViagemRepository : IViagemRepository
{
    // Mesmo formato de data do script.sql
    private const string FormatoData = "yyyy-MM-dd HH:mm:ss";

    private const string SelectBase =
        "SELECT Id, RotaId, MotoristaId, VeiculoId, DataHoraPartida, Tipo, Status FROM Viagens";

    private readonly IConexaoFactory _fabrica;

    public ViagemRepository(IConexaoFactory fabrica)
    {
        _fabrica = fabrica;
    }

    public IEnumerable<Viagem> ObterTodos()
    {
        using var conexao = _fabrica.CriarConexao();
        return conexao.Query<Viagem>(SelectBase + " ORDER BY DataHoraPartida").ToList();
    }

    public Viagem? ObterPorId(int id)
    {
        using var conexao = _fabrica.CriarConexao();
        return conexao.QueryFirstOrDefault<Viagem>(
            SelectBase + " WHERE Id = @Id",
            new { Id = id });
    }

    public int Adicionar(Viagem entidade)
    {
        using var conexao = _fabrica.CriarConexao();
        return conexao.ExecuteScalar<int>(
            @"INSERT INTO Viagens (RotaId, MotoristaId, VeiculoId, DataHoraPartida, Tipo, Status)
              VALUES (@RotaId, @MotoristaId, @VeiculoId, @DataHoraPartida, @Tipo, @Status);
              SELECT last_insert_rowid();",
            Parametros(entidade));
    }

    public void Atualizar(Viagem entidade)
    {
        using var conexao = _fabrica.CriarConexao();
        var linhas = conexao.Execute(
            @"UPDATE Viagens
              SET RotaId = @RotaId, MotoristaId = @MotoristaId, VeiculoId = @VeiculoId,
                  DataHoraPartida = @DataHoraPartida, Tipo = @Tipo, Status = @Status
              WHERE Id = @Id",
            Parametros(entidade));

        if (linhas == 0)
            throw new RegraNegocioException("Viagem não encontrada.");
    }

    public void Remover(int id)
    {
        if (PossuiReservas(id))
            throw new RegraNegocioException(
                "Não é possível excluir: existem reservas vinculadas a esta viagem.");

        using var conexao = _fabrica.CriarConexao();
        var linhas = conexao.Execute(
            "DELETE FROM Viagens WHERE Id = @Id",
            new { Id = id });

        if (linhas == 0)
            throw new RegraNegocioException("Viagem não encontrada.");
    }

    public bool PossuiReservas(int viagemId)
    {
        using var conexao = _fabrica.CriarConexao();
        return conexao.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM Reservas WHERE ViagemId = @Id",
            new { Id = viagemId }) > 0;
    }

    public int ContarReservasAtivas(int viagemId)
    {
        using var conexao = _fabrica.CriarConexao();
        return conexao.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM Reservas WHERE ViagemId = @Id AND Status <> 'Cancelada'",
            new { Id = viagemId });
    }

    // Duas viagens se sobrepõem quando cada uma começa antes de a outra terminar.
    // O fim de cada viagem existente é a partida + a duração da rota dela.
    public bool MotoristaOcupado(int motoristaId, DateTime inicio, DateTime fim, int ignorarViagemId = 0)
    {
        using var conexao = _fabrica.CriarConexao();
        return conexao.ExecuteScalar<int>(
            @"SELECT COUNT(*)
              FROM Viagens v
              INNER JOIN Rotas r ON r.Id = v.RotaId
              WHERE v.MotoristaId = @MotoristaId
                AND v.Status <> 'Cancelada'
                AND v.Id <> @IgnorarId
                AND datetime(v.DataHoraPartida) < datetime(@Fim)
                AND datetime(v.DataHoraPartida, '+' || r.DuracaoMinutos || ' minutes') > datetime(@Inicio)",
            new
            {
                MotoristaId = motoristaId,
                IgnorarId = ignorarViagemId,
                Inicio = inicio.ToString(FormatoData, CultureInfo.InvariantCulture),
                Fim = fim.ToString(FormatoData, CultureInfo.InvariantCulture)
            }) > 0;
    }

    public bool VeiculoOcupado(int veiculoId, DateTime inicio, DateTime fim, int ignorarViagemId = 0)
    {
        using var conexao = _fabrica.CriarConexao();
        return conexao.ExecuteScalar<int>(
            @"SELECT COUNT(*)
              FROM Viagens v
              INNER JOIN Rotas r ON r.Id = v.RotaId
              WHERE v.VeiculoId = @VeiculoId
                AND v.Status <> 'Cancelada'
                AND v.Id <> @IgnorarId
                AND datetime(v.DataHoraPartida) < datetime(@Fim)
                AND datetime(v.DataHoraPartida, '+' || r.DuracaoMinutos || ' minutes') > datetime(@Inicio)",
            new
            {
                VeiculoId = veiculoId,
                IgnorarId = ignorarViagemId,
                Inicio = inicio.ToString(FormatoData, CultureInfo.InvariantCulture),
                Fim = fim.ToString(FormatoData, CultureInfo.InvariantCulture)
            }) > 0;
    }

    // Data, tipo e status vão como texto, no formato que o script.sql usa
    private static object Parametros(Viagem v)
    {
        return new
        {
            v.Id,
            v.RotaId,
            v.MotoristaId,
            v.VeiculoId,
            DataHoraPartida = v.DataHoraPartida.ToString(FormatoData, CultureInfo.InvariantCulture),
            Tipo = v.Tipo.ToString(),
            Status = v.Status.ToString()
        };
    }
}