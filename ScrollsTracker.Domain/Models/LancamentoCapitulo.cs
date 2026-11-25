using ScrollsTracker.Domain.Enum;

namespace ScrollsTracker.Domain.Models;

public sealed class LancamentoCapitulo
{
	public int ObraId { get; set; }
	public Obra Obra { get; set; } = null!;
	public required string Numero { get; set; }
	public EnumSources Fonte { get; set; }
	public DateTime DetectadoEm { get; set; }
}
