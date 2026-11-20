using global::Dapper;
using UniMove.Exceptions;
using UniMove.Models;
using UniMove.Repositories.Interfaces;

namespace UniMove.Repositories.Dapper;

public class VeiculoRepository : IVeiculoRepository
{
    private readonly IConexaoFactory _fabrica;

    public VeiculoRepository(IConexaoFactory fabrica)
    {
        _fabrica = fabrica;
    }

    public IEnumerable<Veiculo> ObterTodos()
    {
        using var conexao = _fabrica.CriarConexao();
        return conexao.Query<Veiculo>(
            "SELECT Id, Placa, Modelo, Capacidade FROM Veiculos ORDER BY Placa").ToList();
    }

    public Veiculo? ObterPorId(int id)
    {
        using var conexao = _fabrica.CriarConexao();
        return conexao.QueryFirstOrDefault<Veiculo>(
            "SELECT Id, Placa, Modelo, Capacidade FROM Veiculos WHERE Id = @Id",
            new { Id = id });
    }

    public int Adicionar(Veiculo entidade)
    {
        if (PlacaJaExiste(entidade.Placa, 0))
            throw new RegraNegocioException("Já existe um veículo cadastrado com esta placa.");

        using var conexao = _fabrica.CriarConexao();
        return conexao.ExecuteScalar<int>(
            @"INSERT INTO Veiculos (Placa, Modelo, Capacidade)
              VALUES (@Placa, @Modelo, @Capacidade);
              SELECT last_insert_rowid();",
            new { entidade.Placa, entidade.Modelo, entidade.Capacidade });
    }

    public void Atualizar(Veiculo entidade)
    {
        if (PlacaJaExiste(entidade.Placa, entidade.Id))
            throw new RegraNegocioException("Já existe um veículo cadastrado com esta placa.");

        using var conexao = _fabrica.CriarConexao();
        var linhas = conexao.Execute(
            @"UPDATE Veiculos
              SET Placa = @Placa, Modelo = @Modelo, Capacidade = @Capacidade
              WHERE Id = @Id",
            new { entidade.Placa, entidade.Modelo, entidade.Capacidade, entidade.Id });

        if (linhas == 0)
            throw new RegraNegocioException("Veículo não encontrado.");
    }

    public void Remover(int id)
    {
        if (PossuiViagens(id))
            throw new RegraNegocioException(
                "Não é possível excluir: existem viagens vinculadas a este veículo.");

        using var conexao = _fabrica.CriarConexao();
        var linhas = conexao.Execute(
            "DELETE FROM Veiculos WHERE Id = @Id",
            new { Id = id });

        if (linhas == 0)
            throw new RegraNegocioException("Veículo não encontrado.");
    }

    public bool PossuiViagens(int veiculoId)
    {
        using var conexao = _fabrica.CriarConexao();
        var total = conexao.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM Viagens WHERE VeiculoId = @Id",
            new { Id = veiculoId });
        return total > 0;
    }

    // Procura outro veículo com a mesma placa. Ao editar, ignora o próprio veículo.
    private bool PlacaJaExiste(string placa, int idIgnorado)
    {
        using var conexao = _fabrica.CriarConexao();
        var total = conexao.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM Veiculos WHERE Placa = @Placa AND Id <> @IdIgnorado",
            new { Placa = placa, IdIgnorado = idIgnorado });
        return total > 0;
    }
}