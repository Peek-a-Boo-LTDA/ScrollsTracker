using AngleSharp;
using AngleSharp.Dom;
using ScrollsTracker.Domain.Enum;
using ScrollsTracker.Domain.Interfaces;
using ScrollsTracker.Domain.Models;
using ScrollsTracker.Domain.Utils;

namespace ScrollsTracker.Infra.Sources;

public class MangaKatanaSource(HttpClient httpClient) : IObraSource
{
	private static readonly Uri Site = new("https://mangakatana.com/");
	public EnumSources SourceName => EnumSources.MangaKatana;

	public async Task<IReadOnlyList<SearchResult>> BuscarAsync(string titulo, CancellationToken cancellationToken)
	{
		using var formulario = new MultipartFormDataContent();
		formulario.Add(new StringContent(titulo), "s");
		formulario.Add(new StringContent("book_name"), "search_by");

		using var resposta = await httpClient.PostAsync(Site, formulario, cancellationToken);
		resposta.EnsureSuccessStatusCode();
		var html = await resposta.Content.ReadAsStringAsync(cancellationToken);
		using var contexto = BrowsingContext.New(Configuration.Default);
		using var documento = await contexto.OpenAsync(req => req.Content(html), cancellationToken);

		return documento.QuerySelectorAll("div.d-cell.text")
			.Select(Mapear).OfType<SearchResult>().ToList();
	}

	private SearchResult? Mapear(IElement item)
	{
		var titulo = item.QuerySelector(".title")?.TextContent.Trim();
		if (string.IsNullOrWhiteSpace(titulo))
			return null;

		var imagem = (item.Closest(".item") ?? item.ParentElement)?.QuerySelector("img");
		var src = imagem?.GetAttribute("data-src") ?? imagem?.GetAttribute("src");
		return new SearchResult(new Obra
		{
			Titulo = titulo,
			TotalCapitulos = StringUtils.ExtrairCapitulo(item.QuerySelector(".chapter a")?.TextContent),
			Imagem = !string.IsNullOrWhiteSpace(src) && Uri.TryCreate(Site, src, out var capa)
				&& capa.Scheme is "https" or "http" ? capa.AbsoluteUri : null
		}, SourceName);
	}
}
