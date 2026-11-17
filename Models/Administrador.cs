using UniMove.Models.Enums;

namespace UniMove.Models;

public class Administrador : Usuario
{
    public override PerfilUsuario Perfil => PerfilUsuario.Administrador;

    public Administrador(string nome, string email, string senhaHash)
        : base(nome, email, senhaHash)
    {
    }

    // Usado pelo Dapper
    private Administrador() { }

    public override bool PodeGerenciarCadastros() => true;

    public override string ObterPainelInicial() => "/Dashboard";
}