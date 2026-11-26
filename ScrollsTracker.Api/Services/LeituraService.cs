using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ScrollsTracker.Api.Requests;
using ScrollsTracker.Domain.Models;
using ScrollsTracker.Infra.Repository.Context;

namespace ScrollsTracker.Api.Services;

public class LeituraService(AppDbContext db, IMapper mapper)
{
    public async Task<List<ObraResponse>> ComLeituraAsync(List<Obra> obras, string usuarioId,
        CancellationToken cancellationToken = default)
    {
        var ids = obras.Select(obra => obra.Id).ToList();
        var leituras = await db.UsuarioObras.AsNoTracking()
            .Where(leitura => leitura.UsuarioId == usuarioId && ids.Contains(leitura.ObraId))
            .ToDictionaryAsync(leitura => leitura.ObraId, cancellationToken);

        return obras.Select(obra =>
        {
            var resposta = mapper.Map<ObraResponse>(obra);
            if (leituras.TryGetValue(obra.Id, out var leitura))
            {
                resposta.UltimoCapituloLido = leitura.UltimoCapituloLido;
                resposta.StatusLeitura = leitura.StatusLeitura;
                resposta.Seguindo = leitura.Seguindo;
            }
            return resposta;
        }).ToList();
    }

    public async Task AtualizarAsync(int obraId, string usuarioId, LeituraRequest progresso)
    {
        var leitura = await ObterOuCriarAsync(obraId, usuarioId);
        leitura.UltimoCapituloLido = progresso.UltimoCapituloLido;
        leitura.StatusLeitura = progresso.StatusLeitura;
        leitura.LeituraAtualizadaEm = DateTime.UtcNow;
    }

    public async Task SeguirAsync(int obraId, string usuarioId, bool seguindo)
    {
        if (seguindo)
        {
            (await ObterOuCriarAsync(obraId, usuarioId)).Seguindo = true;
        }
        else if (await db.UsuarioObras.FindAsync(usuarioId, obraId) is { } leitura)
        {
            leitura.Seguindo = false;
        }
    }

    public async Task<List<ObraResponse>> ContinuarAsync(string usuarioId, CancellationToken cancellationToken)
    {
        var leituras = await (from leitura in db.UsuarioObras.AsNoTracking()
                              join obra in db.Obras.AsNoTracking() on leitura.ObraId equals obra.Id
                              where leitura.UsuarioId == usuarioId && leitura.StatusLeitura == "Lendo"
                              orderby leitura.LeituraAtualizadaEm descending, obra.DataAtualizacao descending, obra.Id
                              select new { Obra = obra, leitura.UltimoCapituloLido, leitura.StatusLeitura, leitura.Seguindo })
            .ToListAsync(cancellationToken);

        return leituras.Where(item => Obra.InterpretarCapitulo(item.Obra.TotalCapitulos) is not > 0
                || Obra.InterpretarCapitulo(item.Obra.TotalCapitulos) > (Obra.InterpretarCapitulo(item.UltimoCapituloLido) ?? 0))
            .Take(6).Select(item =>
            {
                var obra = mapper.Map<ObraResponse>(item.Obra);
                obra.UltimoCapituloLido = item.UltimoCapituloLido;
                obra.StatusLeitura = item.StatusLeitura;
                obra.Seguindo = item.Seguindo;
                return obra;
            }).ToList();
    }

    private async Task<UsuarioObra> ObterOuCriarAsync(int obraId, string usuarioId)
    {
        var leitura = await db.UsuarioObras.FindAsync(usuarioId, obraId);
        if (leitura == null)
        {
            leitura = new UsuarioObra { UsuarioId = usuarioId, ObraId = obraId };
            db.UsuarioObras.Add(leitura);
        }

        return leitura;
    }
}
