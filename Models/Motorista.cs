using UniMove.Exceptions;
using UniMove.Models.Enums;

namespace UniMove.Models;

public class Motorista : Usuario
{
    public string Cnh { get; private set; }
    public string Telefone { get; private set; }

    public override PerfilUsuario Perfil => PerfilUsuario.Motorista;

    public Motorista(string nome, string email, string senhaHash, string cnh, string telefone)
        : base(nome, email, senhaHash)
    {
        Cnh = ValidarCnh(cnh);
        Telefone = ValidarTelefone(telefone);
    }

    // Usado pelo Dapper
    private Motorista()
    {
        Cnh = string.Empty;
        Telefone = string.Empty;
    }

    public void AlterarDados(string nome, string email, string cnh, string telefone)
    {
        AlterarDadosBase(nome, email);
        Cnh = ValidarCnh(cnh);
        Telefone = ValidarTelefone(telefone);
    }

    public override bool PodeGerenciarCadastros() => false;

    public override string ObterPainelInicial() => "/MinhasViagens";

    private static string ValidarCnh(string cnh)
    {
        var numeros = new string((cnh ?? string.Empty).Where(char.IsDigit).ToArray());
        if (numeros.Length != 11)
            throw new RegraNegocioException("A CNH deve ter 11 dígitos.");
        return numeros;
    }

    private static string ValidarTelefone(string telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone))
            throw new RegraNegocioException("O telefone é obrigatório.");
        return telefone.Trim();
    }
}