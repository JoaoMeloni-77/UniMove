using System.ComponentModel.DataAnnotations;
using UniMove.Models;

namespace UniMove.Models.ViewModels;

public class InstituicaoViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome da instituição é obrigatório.")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "A cidade é obrigatória.")]
    [StringLength(60, ErrorMessage = "A cidade deve ter no máximo 60 caracteres.")]
    [Display(Name = "Cidade")]
    public string Cidade { get; set; } = string.Empty;

    public InstituicaoViewModel() { }

    public InstituicaoViewModel(Instituicao instituicao)
    {
        Id = instituicao.Id;
        Nome = instituicao.Nome;
        Cidade = instituicao.Cidade;
    }
}