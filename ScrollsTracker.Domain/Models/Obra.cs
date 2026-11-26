using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json.Serialization;
using ScrollsTracker.Domain.Enum;

namespace ScrollsTracker.Domain.Models
{
	public class Obra
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		public string? Titulo { get; set; }
		public List<string> TitulosAlternativos { get; set; } = [];
		public Dictionary<EnumSources, string> VinculosExternos { get; set; } = [];
		public string? Descricao { get; set; }
		public string? TotalCapitulos { get; set; }
		[JsonConverter(typeof(JsonStringEnumConverter<EnumSources>))]
		public EnumSources? FonteUltimoCapitulo { get; set; }
		public DateTime? UltimoCapituloDetectadoEm { get; set; }
		public string? Imagem { get; set; }
		public string? Status { get; set; }
		[JsonConverter(typeof(JsonStringEnumConverter<EnumSources>))]
		public EnumSources? FonteDescricao { get; set; }
		[JsonConverter(typeof(JsonStringEnumConverter<EnumSources>))]
		public EnumSources? FonteImagem { get; set; }
		[JsonConverter(typeof(JsonStringEnumConverter<EnumSources>))]
		public EnumSources? FonteStatus { get; set; }
		public bool FixarDescricao { get; set; }
		public bool FixarImagem { get; set; }
		public bool FixarStatus { get; set; }
		public DateTime? ConsultaFontesEm { get; set; }
		[JsonIgnore]
		public Dictionary<EnumSources, string> UltimaConsultaFontes { get; set; } = [];
		public DateTime DataAtualizacao { get; set; } = DateTime.Now;
		public DateTime DataVerificacao { get; set; } = DateTime.Now;

		public double GetTotalCapitulosAsDouble()
		{
			return InterpretarCapitulo(TotalCapitulos) is decimal capitulo ? (double)capitulo : 0;
		}

		public static decimal? InterpretarCapitulo(string? valor) =>
			decimal.TryParse(valor?.Trim().Replace(',', '.'), NumberStyles.AllowDecimalPoint,
				CultureInfo.InvariantCulture, out var capitulo) && capitulo >= 0 ? capitulo : null;

		public static string FormatarCapitulo(decimal numero) =>
			numero.ToString("0.############################", CultureInfo.InvariantCulture);
	}
}
