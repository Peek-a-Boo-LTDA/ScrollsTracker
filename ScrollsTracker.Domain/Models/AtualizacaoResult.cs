namespace ScrollsTracker.Domain.Models
{
	public class AtualizacaoResult
	{
		public Obra Obra { get; set; }
		public bool NovoCapitulo { get; set; }

		public AtualizacaoResult(Obra obra, bool novoCapitulo)
		{
			Obra = obra;
			NovoCapitulo = novoCapitulo;
		}
	}
}
