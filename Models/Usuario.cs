using UniMove.Exceptions;
using UniMove.Models.Enums;

namespace UniMove.Models;

public abstract class Usuario
{
    public int Id { get; private set; }
    public string Nome { get; private set; }
    public string Email { get; private set; }
    public string SenhaHash { get; private set; }
    public bool Ativo { get; private set; }

    // Cada subclasse informa o próprio perfil
    public abstract PerfilUsuario Perfil { get; }

    // Comportamento polimórfico: cada perfil tem uma regra e um destino diferentes
    public abstract bool PodeGerenciarCadastros();
    public abstract string ObterPainelInicial();

    // Usado pelo Dapper ao ler do banco
    protected Usuario()
    {
        Nome = string.Empty;
        Email = string.Empty;
        SenhaHash = string.Empty;
    }

    protected Usuario(string nome, string email, string senhaHash)
    {
        Nome = ValidarNome(nome);
        Email = ValidarEmail(email);

        if (string.IsNullOrWhiteSpace(senhaHash))
            throw new RegraNegocioException("A senha é obrigatória.");

        SenhaHash = senhaHash;
        Ativo = true;
    }

    protected void AlterarDadosBase(string nome, string email)
    {
        Nome = ValidarNome(nome);
        Email = ValidarEmail(email);
    }

    public void Desativar() => Ativo = false;
    public void Ativar() => Ativo = true;

    private static string ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new RegraNegocioException("O nome é obrigatório.");
        return nome.Trim();
    }

    private static string ValidarEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new RegraNegocioException("Informe um e-mail válido.");
        return email.Trim().ToLowerInvariant();
    }
}