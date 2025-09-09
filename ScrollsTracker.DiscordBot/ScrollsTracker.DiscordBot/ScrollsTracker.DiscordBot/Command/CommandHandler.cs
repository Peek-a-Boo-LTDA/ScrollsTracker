using Discord;
using Discord.WebSocket;
using ScrollsTracker.DiscordBot.Model.Interfaces;
using ScrollsTracker.DiscordBot.Modules;
using System.Data.SqlTypes;
using System.Diagnostics.Metrics;

namespace ScrollsTracker.DiscordBot.Command
{
	public class CommandHandler
	{
		private readonly DiscordSocketClient _client;
		private readonly ulong _fixedChannelId;
		private readonly SearchModule _searchModule;

		public CommandHandler(DiscordSocketClient client, ulong fixedChannelId, IScrollsTrackerHttpService scrollsTrackerHttpService)
		{
			_client = client;
			_fixedChannelId = fixedChannelId;
			_searchModule = new SearchModule(scrollsTrackerHttpService);
		}

		public Task InitializeAsync()
		{
			_client.MessageReceived += HandleCommandAsync;
			return Task.CompletedTask;
		}

		private async Task HandleCommandAsync(SocketMessage message)
		{
			if (message.Author.IsBot) return;

			string prefix = "!";
			if (!message.Content.StartsWith(prefix)) return;

			// Remove o prefixo da mensagem
			string contentWithoutPrefix = message.Content.Substring(prefix.Length);

			// Divide o conteúdo em palavras
			string[] parts = contentWithoutPrefix.Split(' ', 2);

			// Obtém o comando e o argumento
			string command = parts[0].ToLower(); // Converte para minúsculo para ser case-insensitive
			string arguments = parts.Length > 1 ? parts[1] : null;

			var fixedChannel = _client.GetChannel(_fixedChannelId) as IMessageChannel;

			switch (command)
			{
				case "ping":
					await new PingModule().ExecuteAsync(message, fixedChannel);
					break;

				case "search":
					// Agora, a variável 'arguments' contém o que você precisa
					if (!string.IsNullOrEmpty(arguments))
					{
						await _searchModule.SearchAsync(message, fixedChannel, arguments);
					}
					else
					{
						await fixedChannel.SendMessageAsync("Por favor, forneça um título para a busca. Ex: `!search Nome do Titulo`");
					}
					break;

					// Adicione outros comandos aqui...
			}
		}
	}
}
