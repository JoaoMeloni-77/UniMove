using UniMove.Exceptions;
using UniMove.Models.Enums;

namespace UniMove.Models;

public class Aluno : Usuario
{
    public int InstituicaoId { get; private set; }
    public string Matricula { get; private set; }
    public string Curso { get; private set; }

    public override PerfilUsuario Perfil => PerfilUsuario.Aluno;

    public Aluno(string nome, string email, string senhaHash,
                 int instituicaoId, string matricula, string curso)
        : base(nome, email, senhaHash)
    {
        InstituicaoId = ValidarInstituicao(instituicaoId);
        Matricula = ValidarTexto(matricula, "A matrícula");
        Curso = ValidarTexto(curso, "O curso");
    }

    // Usado pelo Dapper
    private Aluno()
    {
        Matricula = string.Empty;
        Curso = string.Empty;
    }

    public void AlterarDados(string nome, string email, int instituicaoId,
                             string matricula, string curso)
    {
        AlterarDadosBase(nome, email);
        InstituicaoId = ValidarInstituicao(instituicaoId);
        Matricula = ValidarTexto(matricula, "A matrícula");
        Curso = ValidarTexto(curso, "O curso");
    }

    public override bool PodeGerenciarCadastros() => false;

    public override string ObterPainelInicial() => "/Reservas";

    private static int ValidarInstituicao(int instituicaoId)
    {
        if (instituicaoId <= 0)
            throw new RegraNegocioException("O aluno deve estar associado a uma instituição.");
        return instituicaoId;
    }

    private static string ValidarTexto(string valor, string campo)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new RegraNegocioException($"{campo} é obrigatório.");
        return valor.Trim();
    }
}