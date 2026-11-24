using UniMove.Models.ViewModels;

namespace UniMove.Repositories.Interfaces;

public interface IDashboardRepository
{
    IndicadoresViewModel ObterIndicadores();
    IEnumerable<OcupacaoViagemItem> ObterOcupacaoPorViagem();
    IEnumerable<RelatorioLinha> ObterRelatorio(
        int instituicaoId, DateTime? inicio, DateTime? fim, int horaMin, int horaMax);
}