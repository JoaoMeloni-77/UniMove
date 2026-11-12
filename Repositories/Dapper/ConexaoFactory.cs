using System.Data;
using Microsoft.Data.Sqlite;
using UniMove.Repositories.Interfaces;

namespace UniMove.Repositories.Dapper;

public class ConexaoFactory : IConexaoFactory
{
    private readonly string _stringConexao;

    public string CaminhoBanco { get; }

    public ConexaoFactory(IConfiguration configuration)
    {
        var original = configuration.GetConnectionString("UniMove")
            ?? throw new InvalidOperationException(
                "Connection string 'UniMove' não encontrada no appsettings.json.");

        var construtor = new SqliteConnectionStringBuilder(original);
        CaminhoBanco = Path.GetFullPath(construtor.DataSource);
        construtor.DataSource = CaminhoBanco;
        _stringConexao = construtor.ToString();
    }

    public IDbConnection CriarConexao()
    {
        var conexao = new SqliteConnection(_stringConexao);
        conexao.Open();

        using var comando = conexao.CreateCommand();
        comando.CommandText = "PRAGMA foreign_keys = ON;";
        comando.ExecuteNonQuery();

        return conexao;
    }
}