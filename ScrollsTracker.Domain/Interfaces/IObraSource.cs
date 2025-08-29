using ScrollsTracker.Domain.Enum;
using ScrollsTracker.Infra.Model;

namespace ScrollsTracker.Domain.Interfaces
{
    public interface IObraSource
    {
        Task<SearchResult?> ObterObraAsync(string titulo);
        EnumSources SourceName { get; }
	}
}
