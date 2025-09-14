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

		public async Task<bool> CadastrarObraAsync(string titulo)
		{
			if (string.IsNullOrWhiteSpace(titulo))
				throw new ArgumentException("O título não pode ser vazio.", nameof(titulo));

			var url = "https://localhost:7071/api/ScrollsTracker/ProcurarECadastrarObra";
			var request = new HttpRequestMessage(HttpMethod.Post, url)
			{
				Content = new StringContent(JsonConvert.SerializeObject(new { titulo }), System.Text.Encoding.UTF8, "application/json")
			};

			var response = await _httpClient.SendAsync(request);

			return response.IsSuccessStatusCode;
		}

		public async Task<bool> DeletarObraAsync(string titulo)
		{
			if (string.IsNullOrWhiteSpace(titulo))
				throw new ArgumentException("O título não pode ser vazio.", nameof(titulo));

			var obras = await ProcurarObraNoScrollTrackerAsync(titulo);
			var obra = obras.FirstOrDefault(o => string.Equals(o.Titulo, titulo, StringComparison.OrdinalIgnoreCase));

			if (obra == null)
				return false;

			var url = $"https://localhost:7071/api/ScrollsTracker/DeletarObra/{obra.Id}";
			var response = await _httpClient.DeleteAsync(url);

			return response.IsSuccessStatusCode;
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

		public async Task<List<Obra>> ProcurarObraNoScrollTrackerAsync(string titulo)
		{
			if (string.IsNullOrWhiteSpace(titulo))
				throw new ArgumentException("O título não pode ser vazio.", nameof(titulo));

			var url = $"https://localhost:7071/api/ScrollsTracker/ProcurarObras?titulo={Uri.EscapeDataString(titulo)}";
			var response = await _httpClient.GetAsync(url);

			response.EnsureSuccessStatusCode();

			var json = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<List<Obra>>(json)!;
		}
	}

	
}
