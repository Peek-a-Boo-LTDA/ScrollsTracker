using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using ScrollsTracker.Domain.Enum;
using ScrollsTracker.Domain.Interfaces;
using ScrollsTracker.Domain.Models;
using ScrollsTracker.Infra.ExternalApis.DTO.MangaDex.Chapter;
using ScrollsTracker.Infra.ExternalApis.DTO.MangaDex.Search;

namespace ScrollsTracker.Infra.Sources;

public class MangaDexSource(HttpClient httpClient) : IObraSource
{
	private const string Api = "https://api.mangadex.org";
	public EnumSources SourceName => EnumSources.MangaDex;

	public async Task<IReadOnlyList<SearchResult>> BuscarAsync(string titulo, CancellationToken cancellationToken)
	{
		var url = $"{Api}/manga?title={Uri.EscapeDataString(titulo)}&limit=8&includes%5B%5D=cover_art";
		var resposta = await httpClient.GetFromJsonAsync<BaseMangaDexSearchResponse>(url, cancellationToken);
		return resposta?.Data?.Select(Mapear).OfType<SearchResult>().ToList() ?? [];
	}

	public async Task<SearchResult?> ObterPorIdAsync(string id, CancellationToken cancellationToken)
	{
		if (!Guid.TryParseExact(id, "D", out var mangaId))
			return null;

		var resposta = await httpClient.GetFromJsonAsync<MangaDexDetalhes>(
			$"{Api}/manga/{mangaId:D}?includes%5B%5D=cover_art", cancellationToken);
		return resposta?.Data is { } manga ? Mapear(manga) : null;
	}

	public async Task<SearchResult> CompletarAsync(SearchResult resultado, CancellationToken cancellationToken)
	{
		var id = resultado.Obra.VinculosExternos[SourceName];
		var resposta = await httpClient.GetFromJsonAsync<BaseMangaDexChapterResponse>(
			$"{Api}/manga/{id}/feed?limit=10&order%5Bchapter%5D=desc", cancellationToken);

		var ultimo = resposta?.Data?
			.Select(item => Obra.InterpretarCapitulo(item.Attributes?.Chapters))
			.Max();
		resultado.Obra.TotalCapitulos = ultimo?.ToString(CultureInfo.InvariantCulture);
		return resultado;
	}

	private SearchResult? Mapear(MangaDexSearchDataResponse item)
	{
		if (!Guid.TryParseExact(item.Id, "D", out var id) || item.Attributes is not { } atributos)
			return null;

		var titulo = Localizar(atributos.Title);
		if (string.IsNullOrWhiteSpace(titulo))
			return null;

		var capa = item.Relationships?.FirstOrDefault(r => r.Type == "cover_art")?.Attributes?.FileName;
		var referencias = new Dictionary<EnumSources, string>();
		if (atributos.Links?.GetValueOrDefault("mu") is { } mangaUpdates)
			referencias[EnumSources.MangaUpdate] = mangaUpdates;
		if (atributos.Links?.GetValueOrDefault("kt") is { } kitsu)
			referencias[EnumSources.Kitsu] = kitsu;

		var obra = new Obra
		{
			Titulo = titulo,
			TitulosAlternativos = (atributos.Title?.Values.AsEnumerable() ?? [])
				.Concat(atributos.AltTitles?.SelectMany(t => t.Values) ?? []).ToList(),
			Descricao = Localizar(atributos.Description),
			Status = atributos.Status,
			Imagem = string.IsNullOrWhiteSpace(capa) ? null
				: $"https://uploads.mangadex.org/covers/{id:D}/{Uri.EscapeDataString(capa)}.256.jpg",
			VinculosExternos = new() { [SourceName] = id.ToString("D") }
		};
		return new SearchResult(obra, SourceName, atributos.Year, referencias, RequerDetalhes: true, Formato: "Quadrinhos");
	}

	private static string? Localizar(Dictionary<string, string>? valores) =>
		valores?.Values.Prepend(valores.GetValueOrDefault("en")).FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));

	private sealed record MangaDexDetalhes(MangaDexSearchDataResponse? Data);
}
