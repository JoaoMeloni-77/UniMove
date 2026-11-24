using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniMove.Exceptions;
using UniMove.Models;
using UniMove.Models.ViewModels;
using UniMove.Services;

namespace UniMove.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly AutenticacaoService _autenticacao;

    public AccountController(AutenticacaoService autenticacao)
    {
        _autenticacao = autenticacao;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel modelo)
    {
        if (!ModelState.IsValid)
            return View(modelo);

        try
        {
            // Variável do tipo BASE: pode ser Aluno, Motorista ou Administrador
            Usuario usuario = _autenticacao.Autenticar(modelo.Email, modelo.Senha);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new(ClaimTypes.Name, usuario.Nome),
                new(ClaimTypes.Role, usuario.Perfil.ToString()),                          // override
                new("PodeGerenciar", usuario.PodeGerenciarCadastros().ToString()),        // override
                new("PainelInicial", usuario.ObterPainelInicial())                        // override
            };

            var identidade = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identidade));

            if (!string.IsNullOrEmpty(modelo.ReturnUrl) && Url.IsLocalUrl(modelo.ReturnUrl))
                return LocalRedirect(modelo.ReturnUrl);

            return LocalRedirect(usuario.ObterPainelInicial());   // override: cada perfil vai para um lugar
        }
        catch (RegraNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(modelo);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    public IActionResult AccessDenied() => View();
}