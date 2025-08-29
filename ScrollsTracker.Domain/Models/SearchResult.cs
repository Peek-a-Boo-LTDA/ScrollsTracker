using ScrollsTracker.Domain.Enum;
using ScrollsTracker.Domain.Models;

namespace ScrollsTracker.Infra.Model
{
	public class SearchResult
	{
		public Obra Obra { get; set; }
		public int Score { get; set; }
		public EnumSources Source { get; set; }

		public SearchResult(Obra obra, int score, EnumSources source)
		{
			Obra = obra;
			Score = score;
			Source = source;
		}
	}
}
