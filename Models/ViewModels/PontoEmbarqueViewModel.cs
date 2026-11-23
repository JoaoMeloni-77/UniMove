using System.ComponentModel.DataAnnotations;
using UniMove.Models;

namespace UniMove.Models.ViewModels;

public class PontoEmbarqueViewModel
{
    public int Id { get; set; }

    public int Ordem { get; set; }

    [Required(ErrorMessage = "O nome do ponto é obrigatório.")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
    [Display(Name = "Nome do ponto")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(150, ErrorMessage = "A referência deve ter no máximo 150 caracteres.")]
    [Display(Name = "Referência")]
    public string? Referencia { get; set; }

    public PontoEmbarqueViewModel() { }

    public PontoEmbarqueViewModel(PontoEmbarque ponto)
    {
        Id = ponto.Id;
        Ordem = ponto.Ordem;
        Nome = ponto.Nome;
        Referencia = ponto.Referencia;
    }
}