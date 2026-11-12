namespace UniMove.Exceptions;

public class RegraNegocioException : Exception
{
    public RegraNegocioException(string mensagem)
        : base(mensagem)
    {
    }

    public RegraNegocioException(string mensagem, Exception excecaoInterna)
        : base(mensagem, excecaoInterna)
    {
    }
}