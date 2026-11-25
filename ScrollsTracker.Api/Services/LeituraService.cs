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
            }
            return resposta;
        }).ToList();
    }

    public async Task AtualizarAsync(int obraId, string usuarioId, LeituraRequest progresso)
    {
        var leitura = await db.UsuarioObras.FindAsync(usuarioId, obraId);
        if (leitura == null)
        {
            leitura = new UsuarioObra { UsuarioId = usuarioId, ObraId = obraId };
            db.UsuarioObras.Add(leitura);
        }

        leitura.UltimoCapituloLido = progresso.UltimoCapituloLido;
        leitura.StatusLeitura = progresso.StatusLeitura;
    }
}
