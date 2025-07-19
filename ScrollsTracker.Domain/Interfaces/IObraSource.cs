using ScrollsTracker.Domain.Enum;
using ScrollsTracker.Domain.Models;

namespace ScrollsTracker.Domain.Interfaces
{
    public interface IObraSource
    {
        Task<Obra?> ObterObraAsync(string titulo);
        EnumSources SourceName { get; }
	}
}
