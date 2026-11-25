using ScrollsTracker.Domain.Models;

namespace ScrollsTracker.Api.Repository;

public static class ConsultaObras
{
	public static IQueryable<Obra> ComTitulo(this IQueryable<Obra> obras, string? titulo)
	{
		if (string.IsNullOrWhiteSpace(titulo))
			return obras;

		var termo = titulo.Trim().ToUpperInvariant();
		return obras.Where(obra => obra.Titulo!.ToUpper().Contains(termo)
			|| obra.TitulosAlternativos.Any(alias => alias.ToUpper().Contains(termo)));
	}
}
