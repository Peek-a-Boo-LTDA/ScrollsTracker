using ScrollsTracker.Domain.Enum;

namespace ScrollsTracker.Domain.Models;

public sealed record SearchResult(
	Obra Obra,
	EnumSources Source,
	int? Ano = null,
	IReadOnlyDictionary<EnumSources, string>? Referencias = null,
	bool RequerDetalhes = false,
	string? Formato = null);

public sealed record CandidatoObra(Obra Obra, int? Ano, string? Formato, IReadOnlyList<string> Fontes, Guid? Chave = null);

public sealed record BuscaExternaResult(Obra? Obra, IReadOnlyList<CandidatoObra> Candidatos);
