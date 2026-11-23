using System.ComponentModel.DataAnnotations;
using UniMove.Models;

namespace UniMove.Models.ViewModels;

public class RotaViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome da rota é obrigatório.")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = string.Empty;

    [Range(1, 600, ErrorMessage = "A duração deve estar entre 1 e 600 minutos.")]
    [Display(Name = "Duração (minutos)")]
    public int DuracaoMinutos { get; set; }

    // Só para exibir. Os pontos são editados na tela de Detalhes.
    public List<PontoEmbarqueViewModel> Pontos { get; set; } = new();

    public RotaViewModel() { }

    public RotaViewModel(Rota rota)
    {
        Id = rota.Id;
        Nome = rota.Nome;
        DuracaoMinutos = rota.DuracaoMinutos;
        Pontos = rota.Pontos
            .OrderBy(p => p.Ordem)
            .Select(p => new PontoEmbarqueViewModel(p))
            .ToList();
    }
}