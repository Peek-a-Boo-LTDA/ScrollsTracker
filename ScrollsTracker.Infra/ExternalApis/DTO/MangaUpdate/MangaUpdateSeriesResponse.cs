using ScrollsTracker.Domain.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ScrollsTracker.Infra.ExternalApis.DTO.MangaUpdate
{
    public class MangaUpdateSeriesResponse
    {
		[JsonPropertyName("series_id")]
		public long SeriesId { get; set; }

		[JsonPropertyName("title")]
		public string? Title { get; set; }

		[JsonPropertyName("description")]
		public string? Description { get; set; }

		[JsonPropertyName("image")]
		public MangaUpdateSeriesImageResponse? Image { get; set; }

		[JsonPropertyName("type")]
		public string? Type { get; set; }

		[JsonPropertyName("status")]
		public string? Status { get; set; }

		[JsonPropertyName("associated")]
		public List<MangaUpdateAssociatedTitle>? Associated { get; set; }

		[JsonPropertyName("year")]
		public string? Year { get; set; }

		[JsonPropertyName("latest_chapter")]
		public JsonElement LatestChapter { get; set; }

		public Obra ToDomain()
		{
			var capitulo = LatestChapter.ValueKind switch
			{
				JsonValueKind.Number => LatestChapter.GetRawText(),
				JsonValueKind.String => LatestChapter.GetString(),
				_ => null
			};

			return new Obra
			{
				Titulo = Title ?? string.Empty,
				Descricao = Description is null ? string.Empty : Description.Replace("\n", ""),
				Imagem = Image?.Url?.Original,
				TitulosAlternativos = Associated?.Select(i => i.Title).OfType<string>().ToList() ?? [],
				TotalCapitulos = Obra.InterpretarCapitulo(capitulo) is > 0 ? capitulo : null,
				Status = Status ?? string.Empty
			};
		}
	}
}
