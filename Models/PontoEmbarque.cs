using UniMove.Exceptions;

namespace UniMove.Models;

public class PontoEmbarque
{
    public int Id { get; private set; }
    public int RotaId { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string? Referencia { get; private set; }
    public int Ordem { get; private set; }

    // Usado pelo Dapper ao ler do banco
    private PontoEmbarque()
    {
    }

    // internal: só a Rota (mesmo projeto) consegue criar um ponto.
    // É isso que faz do ponto uma parte da rota (composição).
    internal PontoEmbarque(int rotaId, string nome, string? referencia, int ordem)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new RegraNegocioException("O nome do ponto de embarque é obrigatório.");

        if (ordem <= 0)
            throw new RegraNegocioException("A ordem do ponto deve ser maior que zero.");

        RotaId = rotaId;
        Nome = nome.Trim();
        Referencia = string.IsNullOrWhiteSpace(referencia) ? null : referencia.Trim();
        Ordem = ordem;
    }

    internal void DefinirOrdem(int ordem)
    {
        if (ordem <= 0)
            throw new RegraNegocioException("A ordem do ponto deve ser maior que zero.");

        Ordem = ordem;
    }
}