using AngleSharp;
using AngleSharp.Dom;
using FuzzySharp;
using ScrollsTracker.Domain.Enum;
using ScrollsTracker.Domain.Interfaces;
using ScrollsTracker.Domain.Models;
using ScrollsTracker.Domain.Utils;
using ScrollsTracker.Infra.Model;

namespace ScrollsTracker.Infra.Sources
{
	public class MangaKatanaSource : IObraSource
	{
		public EnumSources SourceName => EnumSources.MangaKatana;
		private readonly HttpClient _httpClient;

		public MangaKatanaSource(HttpClient httpClient)
		{
			_httpClient = httpClient;
			if (!_httpClient.DefaultRequestHeaders.UserAgent.Any())
			{
				_httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("ScrollsTracker/1.0 (localhost)");
			}
		}

		private async Task<string> ObterPaginaComDadosDasObrasAsync(string titulo)
		{
			var url = $"https://mangakatana.com/";

			using var formContent = new MultipartFormDataContent();
			formContent.Add(new StringContent(titulo), name: "s");
			formContent.Add(new StringContent("book_name"), name: "search_by");

			var response = await _httpClient.PostAsync(url, formContent);

			response.EnsureSuccessStatusCode();

			return await response.Content.ReadAsStringAsync();
		}

		public async Task<SearchResult?> ObterObraAsync(string titulo)
		{
			var htmlString = await ObterPaginaComDadosDasObrasAsync(titulo);
			if (string.IsNullOrEmpty(htmlString))
			{
				return null;
			}

			return await ObterInformacoesAPartirDoHtml(htmlString, titulo);
		}

		private async Task<SearchResult?> ObterInformacoesAPartirDoHtml(string htmlString, string titulo)
		{
			var config = Configuration.Default.WithDefaultLoader();
			var context = BrowsingContext.New(config);
			var document = await context.OpenAsync(req => req.Content(htmlString));

			var pesquisaHtml = document.QuerySelectorAll("div.d-cell.text");

			int indice = ProcurarMelhorIndiceDaObraPorTitulo(pesquisaHtml, titulo, out var melhorTitulo, out var score);
			var capitulos = ObterCapitulos(pesquisaHtml[indice], indice);

			var obra = new Obra { Titulo = melhorTitulo, TotalCapitulos = capitulos.ToString() };

			return new SearchResult(obra, score, SourceName);
		}

		private string ObterCapitulos(IElement document, int indice)
		{
			var stringCapitulos = document.QuerySelector(".chapter")!
				.GetElementsByTagName("a")
				.FirstOrDefault()!
				.InnerHtml;

			return StringUtils.ManterApenasNumeros(stringCapitulos);
		}

		private int ProcurarMelhorIndiceDaObraPorTitulo(IHtmlCollection<IElement> obras, string titulo, out string melhorTitulo, out int score)
		{
			int indice = 0;
			var melhorPontuacao = 0;
			var melhorPesquisa = 0;
			melhorTitulo = "";

			foreach (var element in obras)
			{
				var tituloEncontrado = element.GetElementsByClassName("title").FirstOrDefault()!.InnerHtml;
				int similaridade = Fuzz.Ratio(tituloEncontrado, titulo);

				if (similaridade >= melhorPontuacao)
				{
					melhorPontuacao = similaridade;
					melhorPesquisa = indice;
					melhorTitulo = tituloEncontrado;
				}

				indice++;
			}

			score = melhorPontuacao;
			return melhorPesquisa;
		}
	}
}
