using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using ScrollsTracker.Domain.Models;

namespace ScrollsTracker.Application.Services;

public sealed class CacheCandidatos : IDisposable
{
	private readonly MemoryCache _cache = new(new MemoryCacheOptions { SizeLimit = 500 });

	public Guid Guardar(string usuarioId, IReadOnlyList<SearchResult> resultados)
	{
		var chave = Guid.NewGuid();
		_cache.Set(chave, new Entrada(usuarioId, JsonSerializer.Serialize(resultados)),
			new MemoryCacheEntryOptions { Size = 1, AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
		return chave;
	}

	public SearchResult[]? Obter(Guid chave, string usuarioId) =>
		_cache.TryGetValue(chave, out Entrada? entrada) && entrada is not null && entrada.UsuarioId == usuarioId
			? JsonSerializer.Deserialize<SearchResult[]>(entrada.Resultados)
			: null;

	public void Dispose() => _cache.Dispose();

	private sealed record Entrada(string UsuarioId, string Resultados);
}
