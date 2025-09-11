namespace ScrollsTracker.DiscordBot.Model
{
	public class Obra
	{
		public int Id { get; set; }
		public string Titulo { get; set; }
		public string Descricao { get; set; }
		public string TotalCapitulos { get; set; }
		public string? UltimoCapituloLido { get; set; }
		public string Imagem { get; set; }
		public string Status { get; set; }
		public string StatusLeitura { get; set; }
		public DateTime DataAtualizacao { get; set; }
		public DateTime DataVerificacao { get; set; }
	}
}
