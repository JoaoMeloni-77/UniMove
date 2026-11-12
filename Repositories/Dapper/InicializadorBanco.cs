using Dapper;
using Microsoft.Data.Sqlite;
using UniMove.Repositories.Interfaces;

namespace UniMove.Repositories.Dapper;

public static class InicializadorBanco
{
    public static void Inicializar(IConexaoFactory fabrica, string caminhoScript)
    {
        if (File.Exists(fabrica.CaminhoBanco))
            return;

        if (!File.Exists(caminhoScript))
            throw new FileNotFoundException(
                $"Script do banco não encontrado em: {caminhoScript}");

        try
        {
            using var conexao = fabrica.CriarConexao();
            using var transacao = conexao.BeginTransaction();

            var script = File.ReadAllText(caminhoScript);
            conexao.Execute(script, transaction: transacao);

            transacao.Commit();
        }
        catch
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(fabrica.CaminhoBanco))
                File.Delete(fabrica.CaminhoBanco);
            throw;
        }
    }
}