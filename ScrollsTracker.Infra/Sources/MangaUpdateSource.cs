using FuzzySharp;
using ScrollsTracker.Domain.Enum;
using ScrollsTracker.Domain.Interfaces;
using ScrollsTracker.Domain.Models;
using ScrollsTracker.Infra.ExternalApis.DTO.MangaUpdate;
using System.Text;
using System.Text.Json;

namespace ScrollsTracker.Infra.Sources
{
	public class MangaUpdateSource : IObraSource
	{
		public EnumSources SourceName => EnumSources.MangaUpdate;
		private readonly HttpClient _httpClient;

		public MangaUpdateSource(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		public async Task<Obra?> ObterObraAsync(string titulo)
		{
			var search = await ProcurarObraAsync(titulo);
			if (search == null || search.Results.Count == 0)
			{
				return null;
			}

			var result = FiltrarResultados(search.Results, titulo);
			//var result = search?.Results.FirstOrDefault();
			if (result == null || result.Record == null)
			{
				return null;
			}

			var serie = await ObterObraPorIdAsync(result.Record.SeriesId);

			return serie?.ToDomain();
		}

		private MangaUpdateResultsResponse? FiltrarResultados(IList<MangaUpdateResultsResponse> results, string titulo)
		{
			var melhorPesquisa = results.FirstOrDefault();
			int melhorPontuacao = 85;

			foreach (var result in results)
			{
				if (result.Record == null || result.Record.SeriesId == 0)
				{
					continue;
				}

				int pontuacao = Fuzz.Ratio(result.Record.Title, titulo);
				if (result.Record.Title != null && pontuacao >= melhorPontuacao)
				{
					melhorPontuacao = pontuacao;
					melhorPesquisa = result;
				}
			}

			return melhorPesquisa ?? null;
		}

		private async Task<MangaUpdateSeriesResponse?> ObterObraPorIdAsync(long id)
		{
			var url = $"https://api.mangaupdates.com/v1/series/{id}";

			var response = await _httpClient.GetAsync(url);
			if (!response.IsSuccessStatusCode)
			{
				var error = await response.Content.ReadAsStringAsync();
				throw new Exception($"Erro {response.StatusCode}: {error}");
			}

			var responseContent = await response.Content.ReadAsStringAsync();
			return JsonSerializer.Deserialize<MangaUpdateSeriesResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		}


		private async Task<MangaUpdateSearchResponse?> ProcurarObraAsync(string titulo)
		{
			var url = "https://api.mangaupdates.com/v1/series/search";

			var requestBody = new { search = titulo };
			var json = JsonSerializer.Serialize(requestBody);
			using var content = new StringContent(json, Encoding.UTF8, "application/json");

			var response = await _httpClient.PostAsync(url, content);
			if (!response.IsSuccessStatusCode)
			{
				var error = await response.Content.ReadAsStringAsync();
				throw new Exception($"Erro {response.StatusCode}: {error}");
			}

			var responseContent = await response.Content.ReadAsStringAsync();
			return JsonSerializer.Deserialize<MangaUpdateSearchResponse>(responseContent);
		}
	}
}
