using UniMove.Models;

namespace UniMove.Services;

public interface IViagemService
{
    int Cadastrar(Viagem viagem);
    void Alterar(Viagem viagem);
}