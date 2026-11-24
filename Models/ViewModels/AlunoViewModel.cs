using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniMove.Models;

namespace UniMove.Models.ViewModels;

public class AlunoViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [StringLength(100, ErrorMessage = "O e-mail deve ter no máximo 100 caracteres.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Selecione uma instituição.")]
    [Display(Name = "Instituição")]
    public int InstituicaoId { get; set; }

    [Required(ErrorMessage = "A matrícula é obrigatória.")]
    [StringLength(30, ErrorMessage = "A matrícula deve ter no máximo 30 caracteres.")]
    [Display(Name = "Matrícula")]
    public string Matricula { get; set; } = string.Empty;

    [Required(ErrorMessage = "O curso é obrigatório.")]
    [StringLength(100, ErrorMessage = "O curso deve ter no máximo 100 caracteres.")]
    [Display(Name = "Curso")]
    public string Curso { get; set; } = string.Empty;

    // Só para exibição (não vem do formulário)
    [ValidateNever]
    public string? InstituicaoNome { get; set; }

    // Opções do <select> (não vem do formulário)
    [ValidateNever]
    public List<SelectListItem> Instituicoes { get; set; } = new();

    public AlunoViewModel() { }

    public AlunoViewModel(Aluno aluno, string? instituicaoNome = null)
    {
        Id = aluno.Id;
        Nome = aluno.Nome;
        Email = aluno.Email;
        InstituicaoId = aluno.InstituicaoId;
        Matricula = aluno.Matricula;
        Curso = aluno.Curso;
        InstituicaoNome = instituicaoNome;
    }
}