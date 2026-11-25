using ScrollsTracker.Domain.Models;

namespace ScrollsTracker.Domain.Interfaces.Repository
{
    public interface IObraRepository
    {
		Task<List<Obra>> ObterTodasObrasAsync();
		Task<List<int>> ObterIdsParaAtualizarAsync(CancellationToken cancellationToken);
		Task<List<LancamentoCapitulo>> ObterLancamentosRecentesAsync(CancellationToken cancellationToken);
		Task<List<LancamentoCapitulo>> ObterHistoricoAsync(int obraId, CancellationToken cancellationToken);
		Task<List<Obra>> ProcurarObrasAsync(string titulo);
		Task<int> AddAsync(Obra obra);
		Task<Obra?> GetObraByIdAsync(int id, CancellationToken cancellationToken = default);
		Task<int> UpdateObraAsync(Obra obra);
		Task<bool> SalvarAtualizacaoAsync(AtualizacaoResult atualizacao, CancellationToken cancellationToken);
		Task<int> CadastrarObraAsync(Obra obra);
		Task<int> CadastrarObrasAsync(List<Obra> obra);
		Task<int> DeleteObraByIdAsync(int id);
	}
}
