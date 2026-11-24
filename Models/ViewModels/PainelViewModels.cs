using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniMove.Models.Enums;

namespace UniMove.Models.ViewModels;

public class IndicadoresViewModel
{
    public int TotalViagens { get; set; }
    public int TotalAlunos { get; set; }
    public int PassageirosPrevistos { get; set; }
    public int CapacidadePrevista { get; set; }

    public double OcupacaoPrevista =>
        CapacidadePrevista == 0 ? 0 : Math.Round(PassageirosPrevistos * 100.0 / CapacidadePrevista, 1);
}

public class OcupacaoViagemItem
{
    public DateTime DataHoraPartida { get; set; }
    public string Rota { get; set; } = string.Empty;
    public int Capacidade { get; set; }
    public int Reservas { get; set; }

    public string Rotulo => $"{DataHoraPartida:dd/MM HH:mm} {Rota}";
}

public class DashboardViewModel
{
    public IndicadoresViewModel Indicadores { get; set; } = new();
    public List<OcupacaoViagemItem> Ocupacao { get; set; } = new();
}

public class RelatorioLinha
{
    public int ViagemId { get; set; }
    public DateTime DataHoraPartida { get; set; }
    public TipoViagem Tipo { get; set; }
    public string Rota { get; set; } = string.Empty;
    public string Instituicao { get; set; } = string.Empty;
    public int Passageiros { get; set; }
}

public class RelatorioViewModel
{
    public int InstituicaoId { get; set; }
    public DateTime? DataInicial { get; set; }
    public DateTime? DataFinal { get; set; }
    public string Turno { get; set; } = "todos";

    [ValidateNever] public List<SelectListItem> Instituicoes { get; set; } = new();
    [ValidateNever] public List<RelatorioLinha> Linhas { get; set; } = new();

    public int TotalPassageiros => Linhas.Sum(l => l.Passageiros);
}