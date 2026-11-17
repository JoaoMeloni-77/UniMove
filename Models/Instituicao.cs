using UniMove.Exceptions;

namespace UniMove.Models;

public class Instituicao
{
    public int Id { get; private set; }
    public string Nome { get; private set; }
    public string Cidade { get; private set; }

    // Construtor para criar uma instituição nova (ainda sem Id)
    public Instituicao(string nome, string cidade)
    {
        Nome = ValidarTexto(nome, "O nome da instituição");
        Cidade = ValidarTexto(cidade, "A cidade");
    }

    // Construtor usado pelo Dapper ao ler do banco
    private Instituicao()
    {
        Nome = string.Empty;
        Cidade = string.Empty;
    }

    // Método de domínio: toda mudança passa por aqui, com validação
    public void AlterarDados(string nome, string cidade)
    {
        Nome = ValidarTexto(nome, "O nome da instituição");
        Cidade = ValidarTexto(cidade, "A cidade");
    }

    private static string ValidarTexto(string valor, string campo)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new RegraNegocioException($"{campo} é obrigatório.");

        return valor.Trim();
    }
}