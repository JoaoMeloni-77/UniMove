using System.Data;

namespace UniMove.Repositories.Interfaces;

public interface IConexaoFactory
{
    string CaminhoBanco { get; }
    IDbConnection CriarConexao();
}