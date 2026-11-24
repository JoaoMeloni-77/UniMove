using UniMove.Exceptions;
using UniMove.Models;
using UniMove.Repositories.Interfaces;

namespace UniMove.Services;

public class AutenticacaoService
{
    private readonly IUsuarioRepository _usuarios;

    public AutenticacaoService(IUsuarioRepository usuarios)
    {
        _usuarios = usuarios;
    }

    public Usuario Autenticar(string email, string senha)
    {
        var usuario = _usuarios.ObterPorEmail(email.Trim());

        if (usuario == null || !usuario.Ativo || !SenhaHasher.Verificar(senha, usuario.SenhaHash))
            throw new RegraNegocioException("E-mail ou senha inválidos.");

        // Primeiro acesso: troca o hash provisório do seed por um hash real
        if (SenhaHasher.PrecisaAtualizar(usuario.SenhaHash))
            _usuarios.AtualizarSenha(usuario.Id, SenhaHasher.Gerar(senha));

        return usuario;
    }
}