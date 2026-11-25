using ScrollsTracker.Domain.Enum;
using ScrollsTracker.Domain.Models;

namespace ScrollsTracker.Domain.Interfaces
{
    public interface IObraSource
    {
		EnumSources SourceName { get; }
		Task<IReadOnlyList<SearchResult>> BuscarAsync(string titulo, CancellationToken cancellationToken);
		Task<SearchResult?> ObterPorIdAsync(string id, CancellationToken cancellationToken) =>
			Task.FromResult<SearchResult?>(null);
		Task<SearchResult> CompletarAsync(SearchResult resultado, CancellationToken cancellationToken) =>
			Task.FromResult(resultado);
	}
}
