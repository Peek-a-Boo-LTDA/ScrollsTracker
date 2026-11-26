using System.Text.Json.Serialization;
using ScrollsTracker.Domain.Enum;

namespace ScrollsTracker.Api.Requests;

public sealed record PaginaBiblioteca(IReadOnlyList<ObraResumoResponse> Obras, int Pagina, int TotalPaginas, int Total);

public sealed record ObraResumoResponse(int Id, string? Titulo, string? Descricao, string? Imagem, string? Status,
	string? TotalCapitulos, string? UltimoCapituloLido, string? StatusLeitura, DateTime DataAtualizacao,
	[property: JsonConverter(typeof(JsonStringEnumConverter<EnumSources>))] EnumSources? FonteUltimoCapitulo,
	bool Seguindo);
