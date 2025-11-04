using AngleSharp;
using AngleSharp.Common;
using AngleSharp.Dom;
using FuzzySharp;
using ScrollsTracker.Domain.Enum;
using ScrollsTracker.Domain.Interfaces;
using ScrollsTracker.Domain.Models;
using ScrollsTracker.Domain.Utils;
using ScrollsTracker.Infra.Model;
using System.Text.RegularExpressions;
using System.Web;

namespace ScrollsTracker.Infra.Sources
{
	public class MangaHereSource : IObraSource
	{
		public EnumSources SourceName => EnumSources.MangaKatana;
		private readonly HttpClient _httpClient;

		public MangaHereSource(HttpClient httpClient)
		{
			_httpClient = httpClient;
			if (!_httpClient.DefaultRequestHeaders.UserAgent.Any())
			{
				_httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("ScrollsTracker/1.0 (localhost)");
			}
		}

		private async Task<string> ObterPaginaComDadosDasObrasAsync(string titulo)
		{
			var baseUrl = $"https://www.mangahere.cc/search";

			string encodedQueryValue = HttpUtility.UrlEncode(titulo);
			var finalUrl = $"{baseUrl}?title={encodedQueryValue}";

			var response = await _httpClient.GetAsync(finalUrl);

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

			var htmlProcura = document.QuerySelectorAll("ul.manga-list-4-list li");
			
			int indice = ProcurarMelhorIndiceDaObraPorTitulo(htmlProcura, titulo, out var melhorTitulo, out var score);

			var capitulos = ObterCapitulos(htmlProcura[indice]);

			var obra = new Obra { Titulo = melhorTitulo, TotalCapitulos = capitulos.ToString()};

			return new SearchResult(obra, score, SourceName);
		}

		private string ObterCapitulos(IElement document)
		{
			var stringCapitulos = document.QuerySelectorAll(".manga-list-4-item-tip")[1]!
				.GetElementsByTagName("a").FirstOrDefault()!.InnerHtml;

			var match = Regex.Match(stringCapitulos, @"Ch\.?\s*([\d\.]+)");
			if (!match.Success)
				return "";

			string numeroCapitulo = match.Groups[1].Value;
			return numeroCapitulo;
		}

		private int ProcurarMelhorIndiceDaObraPorTitulo(IHtmlCollection<IElement> obras, string titulo, out string melhorTitulo, out int score)
		{
			var indice = 0;
			var melhorPontuacao = 0;
			var melhorPesquisa = 0;
			melhorTitulo = "";

			foreach (var element in obras)
			{
				var ancoraTitulo = element.GetElementsByClassName("manga-list-4-item-title").FirstOrDefault()!.GetElementsByTagName("a").FirstOrDefault()!;
				var tituloEncontrado = ancoraTitulo.InnerHtml;
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
