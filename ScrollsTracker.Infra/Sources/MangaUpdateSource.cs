using System.Globalization;
using System.Net.Http.Json;
using ScrollsTracker.Domain.Enum;
using ScrollsTracker.Domain.Interfaces;
using ScrollsTracker.Domain.Models;
using ScrollsTracker.Infra.ExternalApis.DTO.MangaUpdate;

namespace ScrollsTracker.Infra.Sources;

public class MangaUpdateSource(HttpClient httpClient) : IObraSource
{
	private const string Api = "https://api.mangaupdates.com/v1/series";
	public EnumSources SourceName => EnumSources.MangaUpdate;

	public async Task<IReadOnlyList<SearchResult>> BuscarAsync(string titulo, CancellationToken cancellationToken)
	{
		using var resposta = await httpClient.PostAsJsonAsync($"{Api}/search",
			new { search = titulo, perpage = 8 }, cancellationToken);
		resposta.EnsureSuccessStatusCode();

		var busca = await resposta.Content.ReadFromJsonAsync<MangaUpdateSearchResponse>(cancellationToken);
		return busca?.Results?.Select(item => item.Record)
			.Where(item => item is not null && item.SeriesId > 0 && !string.IsNullOrWhiteSpace(item.Title))
			.Select(item => new SearchResult(new Obra
			{
				Titulo = item.Title,
				TitulosAlternativos = item.Associated?.Select(t => t.Title).OfType<string>().ToList() ?? [],
				VinculosExternos = new() { [SourceName] = item.SeriesId.ToString(CultureInfo.InvariantCulture) }
			}, SourceName, LerAno(item.Year), RequerDetalhes: true, Formato: item.Type)).ToList() ?? [];
	}

	public async Task<SearchResult?> ObterPorIdAsync(string id, CancellationToken cancellationToken)
	{
		if (!long.TryParse(id, NumberStyles.None, CultureInfo.InvariantCulture, out var seriesId) || seriesId <= 0)
			return null;

		var serie = await httpClient.GetFromJsonAsync<MangaUpdateSeriesResponse>($"{Api}/{seriesId}", cancellationToken);
		if (string.IsNullOrWhiteSpace(serie?.Title))
			return null;

		var obra = serie.ToDomain();
		obra.VinculosExternos[SourceName] = seriesId.ToString(CultureInfo.InvariantCulture);
		return new SearchResult(obra, SourceName, LerAno(serie.Year), Formato: serie.Type);
	}

	public async Task<SearchResult> CompletarAsync(SearchResult resultado, CancellationToken cancellationToken) =>
		await ObterPorIdAsync(resultado.Obra.VinculosExternos[SourceName], cancellationToken) ?? resultado;

	private static int? LerAno(string? ano) => int.TryParse(ano, out var numero) ? numero : null;
}
