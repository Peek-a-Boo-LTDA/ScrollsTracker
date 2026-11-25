using ScrollsTracker.Domain.Models;

namespace ScrollsTracker.Domain.Interfaces
{
    public interface IObraAggregatorService
    {
		Task<Obra> BuscarObraAgregadaAsync(string titulo);
		Task<BuscaExternaResult> BuscarCandidatosAsync(string titulo, IReadOnlyList<string> titulosAlternativos,
			CancellationToken cancellationToken, string? usuarioParaSelecionar = null);
		Task<Obra?> SelecionarCandidatoAsync(Guid chave, string usuarioId, CancellationToken cancellationToken);

		Task<AtualizacaoResult> BuscarEAtualizaObraAsync(Obra obra, CancellationToken cancellationToken = default);
	}
}
