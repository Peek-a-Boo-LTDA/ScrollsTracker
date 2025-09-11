using ScrollsTracker.Domain.Models;

namespace ScrollsTracker.Domain.Interfaces.Facade
{
    public interface IObraFacade
    {
		Task<List<Obra>> ObterTodasObrasAsync();
		Task<List<Obra>> ObterLancamentosAsync();
		Task<Obra?> GetObraByIdAsync(int id);
		Task<List<Obra>> ProcurarObrasAsync(string titulo);
		Task<int> CadastrarObraAsync(Obra obra);
		Task<int> CadastrarObrasAsync(List<Obra> obra);
		Task<int> UpdateObraAsync(Obra obra);
		Task<int> DeleteObraById(int id);
		Task<Obra> BuscarObraAgregadaAsync(string titulo);
	}
}
