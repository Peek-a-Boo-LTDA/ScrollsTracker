using System.Net.Http.Json;
using ScrollsTracker.Domain.Enum;
using ScrollsTracker.Domain.Interfaces;
using ScrollsTracker.Domain.Models;

namespace ScrollsTracker.Infra.Sources;

public class KitsuSource(HttpClient httpClient) : IObraSource
{
	private const string Api = "https://kitsu.app/api/edge/manga";
	public EnumSources SourceName => EnumSources.Kitsu;

	public async Task<IReadOnlyList<SearchResult>> BuscarAsync(string titulo, CancellationToken cancellationToken)
	{
		var url = $"{Api}?filter%5Btext%5D={Uri.EscapeDataString(titulo)}&page%5Blimit%5D=8";
		var resposta = await httpClient.GetFromJsonAsync<Colecao>(url, cancellationToken);
		return resposta?.Data?.Select(Mapear).OfType<SearchResult>().ToList() ?? [];
	}

	public async Task<SearchResult?> ObterPorIdAsync(string id, CancellationToken cancellationToken)
	{
		if (!long.TryParse(id, out var mangaId) || mangaId <= 0)
			return null;

		var resposta = await httpClient.GetFromJsonAsync<Detalhes>($"{Api}/{mangaId}", cancellationToken);
		return resposta?.Data is { } manga ? Mapear(manga) : null;
	}

	private SearchResult? Mapear(Manga item)
	{
		if (item.Attributes is not { } dados || !long.TryParse(item.Id, out var id) || id <= 0)
			return null;

		var titulos = dados.Titles?.Values.AsEnumerable() ?? [];
		var titulo = titulos.Prepend(dados.CanonicalTitle).FirstOrDefault(t => !string.IsNullOrWhiteSpace(t));
		if (titulo is null)
			return null;

		var obra = new Obra
		{
			Titulo = titulo,
			TitulosAlternativos = titulos.Concat(dados.AbbreviatedTitles ?? []).ToList(),
			Descricao = string.IsNullOrWhiteSpace(dados.Synopsis) ? dados.Description : dados.Synopsis,
			Imagem = dados.PosterImage?.Large ?? dados.PosterImage?.Original ?? dados.PosterImage?.Medium,
			Status = dados.Status,
			VinculosExternos = new() { [SourceName] = item.Id! }
		};
		return new SearchResult(obra, SourceName, DateOnly.TryParseExact(dados.StartDate, "yyyy-MM-dd",
			System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var data)
			? data.Year : null, Formato: dados.Subtype);
	}

	private sealed record Colecao(List<Manga>? Data);
	private sealed record Detalhes(Manga? Data);
	private sealed record Manga(string? Id, Atributos? Attributes);
	private sealed record Atributos(string? CanonicalTitle, Dictionary<string, string>? Titles,
		List<string>? AbbreviatedTitles, string? Synopsis, string? Description, Imagem? PosterImage,
		string? Status, string? StartDate, string? Subtype);
	private sealed record Imagem(string? Large, string? Original, string? Medium);
}
