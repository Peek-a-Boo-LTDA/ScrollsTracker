using Newtonsoft.Json;
using ScrollsTracker.DiscordBot.Model;
using ScrollsTracker.DiscordBot.Model.Interfaces;

namespace ScrollsTracker.DiscordBot.Infra.HttpService
{
	public class ScrollsTrackerHttpService : IScrollsTrackerHttpService
	{
		private readonly HttpClient _httpClient;

		public ScrollsTrackerHttpService(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		public async Task<Obra> ProcurarObraNasApisExternasAsync(string titulo)
		{
			if (string.IsNullOrWhiteSpace(titulo))
				throw new ArgumentException("O título não pode ser vazio.", nameof(titulo));

			var url = $"https://localhost:7071/api/ScrollsTracker/ProcurarObraNasApisExternas?titulo={Uri.EscapeDataString(titulo)}";
			var response = await _httpClient.GetAsync(url);

			response.EnsureSuccessStatusCode();

			var json = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<Obra>(json)!;
		}
	}

	
}
