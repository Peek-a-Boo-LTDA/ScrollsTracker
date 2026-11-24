using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ScrollsTracker.Api.Config;

namespace ScrollsTracker.Api.Controllers;

[ApiController]
[Route("api/conta")]
[Authorize]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class ContaController(
    UserManager<IdentityUser> usuarios,
    SignInManager<IdentityUser> sessao,
    IAuthorizationService autorizacao) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("csrf")]
    public IActionResult Csrf([FromServices] IAntiforgery antiforgery) =>
        Ok(new { token = antiforgery.GetAndStoreTokens(HttpContext).RequestToken });

    [AllowAnonymous]
    [EnableRateLimiting(ContaConfig.LimiteAcesso)]
    [HttpPost("cadastro")]
    public async Task<IActionResult> Cadastrar(Credenciais credenciais)
    {
        var email = credenciais.Email.Trim();
        var usuario = new IdentityUser { UserName = email, Email = email };
        var resultado = await usuarios.CreateAsync(usuario, credenciais.Senha);
        if (!resultado.Succeeded)
        {
            return BadRequest(new
            {
                mensagem = string.Join(" ", resultado.Errors.Select(erro =>
                    erro.Code is "DuplicateUserName" or "DuplicateEmail"
                        ? "Esse e-mail já está cadastrado."
                        : erro.Description).Distinct())
            });
        }

        await sessao.SignInAsync(usuario, isPersistent: false);
        return NoContent();
    }

    [AllowAnonymous]
    [EnableRateLimiting(ContaConfig.LimiteAcesso)]
    [HttpPost("login")]
    public async Task<IActionResult> Entrar(Credenciais credenciais)
    {
        var resultado = await sessao.PasswordSignInAsync(
            credenciais.Email.Trim(), credenciais.Senha, isPersistent: false, lockoutOnFailure: true);

        return resultado.Succeeded ? NoContent() : Unauthorized(new { mensagem = "E-mail ou senha inválidos." });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Sair()
    {
        await sessao.SignOutAsync();
        return NoContent();
    }

    [HttpGet("sessao")]
    public async Task<IActionResult> Sessao()
    {
        var podeCadastrar = await autorizacao.AuthorizeAsync(User, ContaConfig.PodeCadastrarObras);
        var podeGerenciar = await autorizacao.AuthorizeAsync(User, ContaConfig.PodeGerenciarObras);
        return Ok(new
        {
            email = User.Identity!.Name,
            podeCadastrarObras = podeCadastrar.Succeeded,
            podeGerenciarObras = podeGerenciar.Succeeded
        });
    }

    public record Credenciais(
        [Required, EmailAddress, StringLength(256)] string Email,
        [Required, StringLength(128)] string Senha);
}
