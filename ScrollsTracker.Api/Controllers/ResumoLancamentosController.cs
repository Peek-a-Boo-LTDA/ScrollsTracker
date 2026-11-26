using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScrollsTracker.Infra.Repository.Context;

namespace ScrollsTracker.Api.Controllers;

[ApiController]
[Authorize]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[Route("api/conta/resumo-lancamentos")]
public sealed class ResumoLancamentosController(AppDbContext db) : ControllerBase
{
    private const string Desativado = "desativado";
    private string UsuarioId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> Obter(CancellationToken cancellationToken)
    {
        var preferencia = await db.PreferenciasResumo.FindAsync([UsuarioId], cancellationToken);
        var frequencia = preferencia?.Frequencia ?? Desativado;
        if (frequencia == Desativado)
            return Ok(new { frequencia, obras = Array.Empty<object>() });

        var desde = DateTime.UtcNow.AddDays(frequencia == "diario" ? -1 : -7);
        var capitulos = await (from lancamento in db.Lancamentos.AsNoTracking()
                               join leitura in db.UsuarioObras.AsNoTracking() on lancamento.ObraId equals leitura.ObraId
                               where leitura.UsuarioId == UsuarioId && leitura.Seguindo && lancamento.DetectadoEm >= desde
                               orderby lancamento.DetectadoEm descending
                               select new { lancamento.ObraId, lancamento.Obra.Titulo, lancamento.Numero, lancamento.DetectadoEm })
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            frequencia,
            obras = capitulos.GroupBy(item => new { item.ObraId, item.Titulo })
                .Select(grupo => new { grupo.Key.ObraId, grupo.Key.Titulo, quantidade = grupo.Count(), ultimo = grupo.First() })
        });
    }

    [HttpPut]
    public async Task<IActionResult> Configurar(ConfiguracaoResumo configuracao, CancellationToken cancellationToken)
    {
        var preferencia = await db.PreferenciasResumo.FindAsync([UsuarioId], cancellationToken);
        if (preferencia == null)
        {
            db.PreferenciasResumo.Add(new PreferenciaResumo { UsuarioId = UsuarioId, Frequencia = configuracao.Frequencia });
        }
        else
        {
            preferencia.Frequencia = configuracao.Frequencia;
        }

        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return NoContent();
        }
        catch (DbUpdateException)
        {
            return Conflict("Não foi possível guardar a preferência. Tente novamente.");
        }
    }

    public sealed record ConfiguracaoResumo([Required, AllowedValues(Desativado, "diario", "semanal")] string Frequencia);
}
