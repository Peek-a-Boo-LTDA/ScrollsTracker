using ScrollsTracker.Domain.Models;

namespace ScrollsTracker.Domain.Interfaces.Repository
{
    public interface IObraRepository
    {
		Task<List<Obra>> ObterTodasObrasAsync();
		Task<List<Obra>> ObterTodasObrasParaAtualizarAsync();
		Task<List<Obra>> ObterLancamentosAsync();
		Task<List<Obra>> ProcurarObrasAsync(string titulo);
		Task<int> AddAsync(Obra obra);
		Task<Obra?> GetObraByIdAsync(int id);
		Task<int> UpdateObraAsync(Obra obra);
		Task<int> CadastrarObraAsync(Obra obra);
		Task<int> CadastrarObrasAsync(List<Obra> obra);
		Task<int> DeleteObraByIdAsync(int id);
	}
}
