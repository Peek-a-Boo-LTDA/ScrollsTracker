using Discord;
using Discord.WebSocket;

namespace ScrollsTracker.DiscordBot.Modules
{
	public class PingModule
	{
		public async Task ExecuteAsync(SocketMessage message, IMessageChannel? fixedChannel)
		{
			if (fixedChannel != null)
			{
				await fixedChannel.SendMessageAsync($"{message.Author.Mention} pediu: Pong! 🏓");
			}
		}
	}
}
