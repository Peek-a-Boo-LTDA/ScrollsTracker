using AngleSharp;
using AngleSharp.Dom;
using ScrollsTracker.Domain.Enum;
using ScrollsTracker.Domain.Interfaces;
using ScrollsTracker.Domain.Models;
using ScrollsTracker.Domain.Utils;

namespace ScrollsTracker.Infra.Sources;

public class MangaHereSource(HttpClient httpClient) : IObraSource
{
	public EnumSources SourceName => EnumSources.MangaHere;

	public async Task<IReadOnlyList<SearchResult>> BuscarAsync(string titulo, CancellationToken cancellationToken)
	{
		var url = $"https://www.mangahere.cc/search?title={Uri.EscapeDataString(titulo)}";
		var html = await httpClient.GetStringAsync(url, cancellationToken);
		using var contexto = BrowsingContext.New(Configuration.Default);
		using var documento = await contexto.OpenAsync(req => req.Content(html), cancellationToken);

		return documento.QuerySelectorAll("ul.manga-list-4-list li")
			.Select(Mapear).OfType<SearchResult>().ToList();
	}

	private SearchResult? Mapear(IElement item)
	{
		var titulo = item.QuerySelector(".manga-list-4-item-title a")?.TextContent.Trim();
		if (string.IsNullOrWhiteSpace(titulo))
			return null;

		var capitulo = item.QuerySelectorAll(".manga-list-4-item-tip a")
			.Select(link => StringUtils.ExtrairCapitulo(link.TextContent)).FirstOrDefault(valor => valor != null);
		return new SearchResult(new Obra { Titulo = titulo, TotalCapitulos = capitulo }, SourceName);
	}
}
