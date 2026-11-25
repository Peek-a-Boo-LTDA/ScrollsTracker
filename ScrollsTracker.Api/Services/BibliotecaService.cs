using Microsoft.EntityFrameworkCore;
using ScrollsTracker.Api.Repository;
using ScrollsTracker.Api.Requests;
using ScrollsTracker.Infra.Repository.Context;

namespace ScrollsTracker.Api.Services;

public sealed class BibliotecaService(AppDbContext db)
{
	public async Task<PaginaBiblioteca> BuscarAsync(string usuarioId, string? titulo, string? statusObra,
		string? statusLeitura, int pagina, int tamanho, CancellationToken cancellationToken)
	{
		var obras = db.Obras.AsNoTracking().ComTitulo(titulo);
		if (!string.IsNullOrWhiteSpace(statusObra))
		{
			var status = statusObra.Trim().ToUpperInvariant();
			obras = obras.Where(obra => obra.Status != null && obra.Status.ToUpper().Contains(status));
		}

		var leituras = db.UsuarioObras.AsNoTracking().Where(leitura => leitura.UsuarioId == usuarioId);
		var consulta = from obra in obras
					   join leitura in leituras on obra.Id equals leitura.ObraId into progresso
					   from leitura in progresso.DefaultIfEmpty()
					   select new { obra, leitura };

		if (!string.IsNullOrWhiteSpace(statusLeitura))
			consulta = consulta.Where(item => item.leitura != null && item.leitura.StatusLeitura == statusLeitura);

		var total = await consulta.CountAsync(cancellationToken);
		var totalPaginas = Math.Max(1, (total + tamanho - 1) / tamanho);
		pagina = Math.Min(pagina, totalPaginas);
		var itens = await consulta.OrderBy(item => item.obra.Titulo).ThenBy(item => item.obra.Id)
			.Skip((pagina - 1) * tamanho).Take(tamanho)
			.Select(item => new ObraResumoResponse(item.obra.Id, item.obra.Titulo, item.obra.Descricao,
				item.obra.Imagem, item.obra.Status, item.obra.TotalCapitulos,
				item.leitura == null ? null : item.leitura.UltimoCapituloLido,
				item.leitura == null ? null : item.leitura.StatusLeitura,
				item.obra.DataAtualizacao, item.obra.FonteUltimoCapitulo))
			.ToListAsync(cancellationToken);

		return new PaginaBiblioteca(itens, pagina, totalPaginas, total);
	}
}
