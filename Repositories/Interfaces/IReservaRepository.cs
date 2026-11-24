using UniMove.Models;
using UniMove.Models.ViewModels;

namespace UniMove.Repositories.Interfaces;

public interface IReservaRepository : IRepository<Reserva>
{
    ViagemParaReserva? ObterDadosViagem(int viagemId);
    IEnumerable<ViagemDisponivelViewModel> ObterViagensDisponiveis(int alunoId);
    IEnumerable<MinhaReservaViewModel> ObterReservasDoAluno(int alunoId);
    IEnumerable<PassageiroViewModel> ObterPassageirosDaViagem(int viagemId);
    IEnumerable<PontoOpcao> ObterPontosDaViagem(int viagemId);
    bool PontoPertenceARota(int pontoId, int rotaId);
    bool AlunoJaReservou(int alunoId, int viagemId);
}