using System.ComponentModel.DataAnnotations;

namespace ScrollsTracker.Api.Requests
{
	public class ObraRequest : LeituraRequest
	{
		[Required]
		public string? Titulo { get; set; }
		public string? Descricao { get; set; }
		public string? TotalCapitulos { get; set; }
		public string? Imagem { get; set; }
		public string? Status { get; set; }
		public DateTime DataAtualizacao { get; set; } = DateTime.Now;
		public DateTime DataVerificacao { get; set; } = DateTime.Now;
	}
}
