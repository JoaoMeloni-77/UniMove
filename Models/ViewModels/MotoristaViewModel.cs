using System.ComponentModel.DataAnnotations;
using UniMove.Models;

namespace UniMove.Models.ViewModels;

public class MotoristaViewModel
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

    [Required(ErrorMessage = "A CNH é obrigatória.")]
    [Display(Name = "CNH")]
    public string Cnh { get; set; } = string.Empty;

    [Required(ErrorMessage = "O telefone é obrigatório.")]
    [StringLength(20, ErrorMessage = "O telefone deve ter no máximo 20 caracteres.")]
    [Display(Name = "Telefone")]
    public string Telefone { get; set; } = string.Empty;

    public MotoristaViewModel() { }

    public MotoristaViewModel(Motorista motorista)
    {
        Id = motorista.Id;
        Nome = motorista.Nome;
        Email = motorista.Email;
        Cnh = motorista.Cnh;
        Telefone = motorista.Telefone;
    }
}