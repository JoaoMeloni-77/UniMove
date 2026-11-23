using System.ComponentModel.DataAnnotations;
using UniMove.Models;

namespace UniMove.Models.ViewModels;

public class VeiculoViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "A placa é obrigatória.")]
    [StringLength(10, ErrorMessage = "A placa deve ter no máximo 10 caracteres.")]
    [Display(Name = "Placa")]
    public string Placa { get; set; } = string.Empty;

    [Required(ErrorMessage = "O modelo é obrigatório.")]
    [StringLength(60, ErrorMessage = "O modelo deve ter no máximo 60 caracteres.")]
    [Display(Name = "Modelo")]
    public string Modelo { get; set; } = string.Empty;

    [Range(1, 100, ErrorMessage = "A capacidade deve estar entre 1 e 100 lugares.")]
    [Display(Name = "Capacidade (lugares)")]
    public int Capacidade { get; set; }

    public VeiculoViewModel() { }

    public VeiculoViewModel(Veiculo veiculo)
    {
        Id = veiculo.Id;
        Placa = veiculo.Placa;
        Modelo = veiculo.Modelo;
        Capacidade = veiculo.Capacidade;
    }
}