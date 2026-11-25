using System.Text.Json.Serialization;

namespace ScrollsTracker.Infra.ExternalApis.DTO.MangaDex.Search
{
	public class MangaDexSearchDataResponse
	{
		[JsonPropertyName("id")]
		public required string Id { get; set; }

		[JsonPropertyName("attributes")]
		public MangaDexSearchAttributesResponse? Attributes { get; set; }

		[JsonPropertyName("relationships")]
		public List<MangaDexRelationship>? Relationships { get; set; }
	}

	public sealed record MangaDexRelationship(string Type, MangaDexCoverFile? Attributes);
	public sealed record MangaDexCoverFile(string? FileName);
}
