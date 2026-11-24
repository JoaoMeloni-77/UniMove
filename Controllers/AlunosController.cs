using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniMove.Exceptions;
using UniMove.Models;
using UniMove.Models.ViewModels;
using UniMove.Repositories.Interfaces;

namespace UniMove.Controllers;

public class AlunosController : Controller
{
    private const string SenhaProvisoria = "HASH_PROVISORIO";

    private readonly IAlunoRepository _repositorio;
    private readonly IInstituicaoRepository _instituicoes;

    public AlunosController(IAlunoRepository repositorio, IInstituicaoRepository instituicoes)
    {
        _repositorio = repositorio;
        _instituicoes = instituicoes;
    }

    public IActionResult Index()
    {
        var nomes = _instituicoes.ObterTodos().ToDictionary(i => i.Id, i => i.Nome);

        var lista = _repositorio.ObterTodos()
            .Select(a => new AlunoViewModel(a, nomes.GetValueOrDefault(a.InstituicaoId)))
            .ToList();
        return View(lista);
    }

    public IActionResult Details(int id)
    {
        var aluno = _repositorio.ObterPorId(id);
        if (aluno == null)
            return NaoEncontrado();

        return View(CriarViewModel(aluno));
    }

    public IActionResult Create()
    {
        var modelo = new AlunoViewModel();
        PreencherInstituicoes(modelo);
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(AlunoViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            PreencherInstituicoes(modelo);
            return View(modelo);
        }

        try
        {
            var aluno = new Aluno(modelo.Nome, modelo.Email, SenhaProvisoria,
                                  modelo.InstituicaoId, modelo.Matricula, modelo.Curso);
            _repositorio.Adicionar(aluno);

            TempData["Sucesso"] = "Aluno cadastrado com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (RegraNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            PreencherInstituicoes(modelo);
            return View(modelo);
        }
    }

    public IActionResult Edit(int id)
    {
        var aluno = _repositorio.ObterPorId(id);
        if (aluno == null)
            return NaoEncontrado();

        var modelo = new AlunoViewModel(aluno);
        PreencherInstituicoes(modelo);
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, AlunoViewModel modelo)
    {
        if (id != modelo.Id)
            return NaoEncontrado();

        if (!ModelState.IsValid)
        {
            PreencherInstituicoes(modelo);
            return View(modelo);
        }

        try
        {
            var aluno = _repositorio.ObterPorId(id);
            if (aluno == null)
                return NaoEncontrado();

            aluno.AlterarDados(modelo.Nome, modelo.Email, modelo.InstituicaoId,
                               modelo.Matricula, modelo.Curso);
            _repositorio.Atualizar(aluno);

            TempData["Sucesso"] = "Aluno atualizado com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (RegraNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            PreencherInstituicoes(modelo);
            return View(modelo);
        }
    }

    public IActionResult Delete(int id)
    {
        var aluno = _repositorio.ObterPorId(id);
        if (aluno == null)
            return NaoEncontrado();

        return View(CriarViewModel(aluno));
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmado(int id)
    {
        try
        {
            _repositorio.Remover(id);
            TempData["Sucesso"] = "Aluno excluído com sucesso.";
        }
        catch (RegraNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private AlunoViewModel CriarViewModel(Aluno aluno)
    {
        var instituicao = _instituicoes.ObterPorId(aluno.InstituicaoId);
        return new AlunoViewModel(aluno, instituicao?.Nome);
    }

    private void PreencherInstituicoes(AlunoViewModel modelo)
    {
        modelo.Instituicoes = _instituicoes.ObterTodos()
            .Select(i => new SelectListItem(i.Nome, i.Id.ToString()))
            .ToList();
    }

    private IActionResult NaoEncontrado()
    {
        TempData["Erro"] = "Aluno não encontrado.";
        return RedirectToAction(nameof(Index));
    }
}