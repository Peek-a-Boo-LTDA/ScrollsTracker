using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ScrollsTracker.Domain.Models
{
	public class Obra
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		public string? Titulo { get; set; }
		public string? Descricao { get; set; }
		public string? TotalCapitulos { get; set; }
		public string? UltimoCapituloLido { get; set; }
		public string? Imagem { get; set; }
		public string? Status { get; set; }
		public string? StatusLeitura { get; set; }
		public DateTime DataAtualizacao { get; set; } = DateTime.Now;
		public DateTime DataVerificacao { get; set; } = DateTime.Now;

		public double GetTotalCapitulosAsDouble()
		{
			if (string.IsNullOrEmpty(TotalCapitulos))
			{
				return 0;
			}
			if (double.TryParse(TotalCapitulos, out double result))
			{
				return result;
			}
			return 0;
		}

		public double GetUltimoCapituloLidoAsDouble()
		{
			if (string.IsNullOrEmpty(UltimoCapituloLido))
			{
				return 0;
			}
			if (double.TryParse(UltimoCapituloLido, out double result))
			{
				return result;
			}
			return 0;
		}
	}
}
