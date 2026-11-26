using ScrollsTracker.Domain.Interfaces;
using ScrollsTracker.Domain.Interfaces.Facade;
using ScrollsTracker.Domain.Interfaces.Repository;
using ScrollsTracker.Domain.Models;

namespace ScrollsTracker.Application.Facade
{
	public class ObraFacade : IObraFacade
	{
		private readonly IObraRepository _repo;
		private readonly IObraAggregatorService _obraAggregator;

		public ObraFacade(IObraRepository repo, IObraAggregatorService obraAggregator)
		{
			_repo = repo;
			_obraAggregator = obraAggregator;
		}

		public Task<Obra> BuscarObraAgregadaAsync(string titulo) => _obraAggregator.BuscarObraAgregadaAsync(titulo);

		public Task<BuscaExternaResult> BuscarCandidatosAsync(string titulo, IReadOnlyList<string> titulosAlternativos,
			CancellationToken cancellationToken, string? usuarioParaSelecionar = null, bool mostrarOpcoes = false) =>
			_obraAggregator.BuscarCandidatosAsync(titulo, titulosAlternativos, cancellationToken, usuarioParaSelecionar, mostrarOpcoes);

		public Task<Obra?> SelecionarCandidatoAsync(Guid chave, string usuarioId, CancellationToken cancellationToken) =>
			_obraAggregator.SelecionarCandidatoAsync(chave, usuarioId, cancellationToken);

		public Task<int> DeleteObraById(int id) => _repo.DeleteObraByIdAsync(id);

		public Task<Obra?> GetObraByIdAsync(int id) => _repo.GetObraByIdAsync(id);

		public Task<List<LancamentoCapitulo>> ObterLancamentosRecentesAsync(CancellationToken cancellationToken) =>
			_repo.ObterLancamentosRecentesAsync(cancellationToken);

		public Task<List<LancamentoCapitulo>> ObterHistoricoAsync(int obraId, CancellationToken cancellationToken) =>
			_repo.ObterHistoricoAsync(obraId, cancellationToken);

		public Task<List<Obra>> ObterTodasObrasAsync() => _repo.ObterTodasObrasAsync();

		public Task<List<Obra>> ProcurarObrasAsync(string titulo) => _repo.ProcurarObrasAsync(titulo);

		public async Task<int> UpdateObraAsync(Obra obraAtualizada) => await _repo.UpdateObraAsync(obraAtualizada);

		public Task<int> CadastrarObraAsync(Obra obra) => _repo.CadastrarObraAsync(obra);
		public Task<int> CadastrarObrasAsync(List<Obra> obras) => _repo.CadastrarObrasAsync(obras);
	}
}
