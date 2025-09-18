using ScrollsTracker.DiscordBot.Model;
using System.Threading.Channels;

namespace ScrollsTracker.DiscordBot.Canais
{
	public static class ObraChannel
	{
		public static Channel<Obra> ChObra { get; } = Channel.CreateUnbounded<Obra>();
	}
}
