using System.ComponentModel.DataAnnotations;

using ScrollsTracker.Domain.Enum;
using System.Text.Json.Serialization;

namespace ScrollsTracker.Api.Requests
{
	public class ObraRequest : LeituraRequest
	{
		[Required]
		public string? Titulo { get; set; }
		[MaxLength(50)]
		public List<string>? TitulosAlternativos { get; set; }
		public Dictionary<EnumSources, string>? VinculosExternos { get; set; }
		public string? Descricao { get; set; }
		public string? TotalCapitulos { get; set; }
		[JsonConverter(typeof(JsonStringEnumConverter<EnumSources>))]
		public EnumSources? FonteUltimoCapitulo { get; set; }
		public string? Imagem { get; set; }
		public string? Status { get; set; }
		[JsonConverter(typeof(JsonStringEnumConverter<EnumSources>))]
		public EnumSources? FonteDescricao { get; set; }
		[JsonConverter(typeof(JsonStringEnumConverter<EnumSources>))]
		public EnumSources? FonteImagem { get; set; }
		[JsonConverter(typeof(JsonStringEnumConverter<EnumSources>))]
		public EnumSources? FonteStatus { get; set; }
		public bool? FixarDescricao { get; set; }
		public bool? FixarImagem { get; set; }
		public bool? FixarStatus { get; set; }
		public DateTime DataAtualizacao { get; set; } = DateTime.Now;
		public DateTime DataVerificacao { get; set; } = DateTime.Now;
	}
}
