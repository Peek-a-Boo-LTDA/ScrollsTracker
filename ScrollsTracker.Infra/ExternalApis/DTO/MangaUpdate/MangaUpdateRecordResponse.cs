using System.Text.Json.Serialization;

namespace ScrollsTracker.Infra.ExternalApis.DTO.MangaUpdate
{
	public class MangaUpdateRecordResponse
	{
		[JsonPropertyName("series_id")]
		public long SeriesId { get; set; }

		[JsonPropertyName("title")]
		public string? Title { get; set; }

		[JsonPropertyName("year")]
		public string? Year { get; set; }

		[JsonPropertyName("type")]
		public string? Type { get; set; }

		[JsonPropertyName("associated")]
		public List<MangaUpdateAssociatedTitle>? Associated { get; set; }
	}

	public sealed record MangaUpdateAssociatedTitle(string? Title);
}
