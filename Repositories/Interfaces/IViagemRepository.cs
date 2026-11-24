using UniMove.Models;
using UniMove.Models.ViewModels;

namespace UniMove.Repositories.Interfaces;

public interface IViagemRepository : IRepository<Viagem>
{
    bool PossuiReservas(int viagemId);
    int ContarReservasAtivas(int viagemId);
    bool MotoristaOcupado(int motoristaId, DateTime inicio, DateTime fim, int ignorarViagemId = 0);
    bool VeiculoOcupado(int veiculoId, DateTime inicio, DateTime fim, int ignorarViagemId = 0);

    IEnumerable<ViagemMotoristaViewModel> ObterViagensDoMotorista(int motoristaId);
    ViagemMotoristaViewModel? ObterResumo(int viagemId);
    IEnumerable<PassageiroEmbarqueViewModel> ObterPassageiros(int viagemId);
}