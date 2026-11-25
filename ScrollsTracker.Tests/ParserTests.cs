using System.Net;
using ScrollsTracker.Domain.Interfaces;
using ScrollsTracker.Infra.Sources;

namespace ScrollsTracker.Tests;

public class ParserTests
{
	[Theory]
	[InlineData(true, """<div class="item"><img src="/capa.jpg"><div class="d-cell text"><a class="title">Amanhecer</a><div class="chapter"><a>Chapter 12.5</a></div></div></div>""")]
	[InlineData(false, """<ul class="manga-list-4-list"><li><p class="manga-list-4-item-title"><a>Amanhecer</a></p><p class="manga-list-4-item-tip"><a>Ch. 12.5</a></p></li></ul>""")]
	public async Task ParserExtraiTituloECapituloDecimal(bool katana, string html)
	{
		using var http = new HttpClient(new RespostaHtml(html));
		IObraSource fonte = katana ? new MangaKatanaSource(http) : new MangaHereSource(http);

		var item = Assert.Single(await fonte.BuscarAsync("Amanhecer", CancellationToken.None));

		Assert.Equal("Amanhecer", item.Obra.Titulo);
		Assert.Equal("12.5", item.Obra.TotalCapitulos);
	}

	private sealed class RespostaHtml(string html) : HttpMessageHandler
	{
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
			Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(html) });
	}
}
