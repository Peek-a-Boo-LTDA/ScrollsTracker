using ScrollsTracker.Domain.Enum;
using ScrollsTracker.Infra.Model;

namespace ScrollsTracker.Domain.Interfaces
{
    public interface IObraFilter
    {
		void Filtrar(SearchResult search, bool obraNova = false);
	}
}
