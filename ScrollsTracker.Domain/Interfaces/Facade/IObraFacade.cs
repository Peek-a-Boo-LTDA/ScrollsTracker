using ScrollsTracker.Domain.Models;

namespace ScrollsTracker.Domain.Interfaces.Facade
{
    public interface IObraFacade
    {
		Task<List<Obra>> ObterTodasObrasAsync();
		Task<List<LancamentoCapitulo>> ObterLancamentosRecentesAsync(CancellationToken cancellationToken);
		Task<List<LancamentoCapitulo>> ObterHistoricoAsync(int obraId, CancellationToken cancellationToken);
		Task<Obra?> GetObraByIdAsync(int id);
		Task<List<Obra>> ProcurarObrasAsync(string titulo);
		Task<int> CadastrarObraAsync(Obra obra);
		Task<int> CadastrarObrasAsync(List<Obra> obra);
		Task<int> UpdateObraAsync(Obra obra);
		Task<int> DeleteObraById(int id);
		Task<Obra> BuscarObraAgregadaAsync(string titulo);
		Task<BuscaExternaResult> BuscarCandidatosAsync(string titulo, IReadOnlyList<string> titulosAlternativos,
			CancellationToken cancellationToken, string? usuarioParaSelecionar = null, bool mostrarOpcoes = false);
		Task<Obra?> SelecionarCandidatoAsync(Guid chave, string usuarioId, CancellationToken cancellationToken);
	}
}
