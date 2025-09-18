using Discord;
using Discord.WebSocket;
using ScrollsTracker.DiscordBot.Modules;
using ScrollsTracker.DiscordBot.Services;

namespace ScrollsTracker.DiscordBot.Command
{
	public class CommandHandler
	{
		private DiscordSocketClient? _client;
		private ulong? _fixedChannel;
		private readonly ObrasModule _searchModule;

		public CommandHandler(ObrasModule obrasModule)
		{
			_searchModule = obrasModule;
		}

		public Task InitializeAsync(BotService botService)
		{
			_client = botService.Client ?? throw new ArgumentNullException(nameof(botService.channel), "BotService.client cannot be null."); ;
			_fixedChannel = botService.FixedChannelId;
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

			var channel = _client!.GetChannel(_fixedChannel!.Value) as IMessageChannel;

			if (channel == null)
				throw new Exception("Falha ao obter channel");

			switch (command)
			{
				case "ping":
					await new PingModule().ExecuteAsync(message, channel);
					break;

				case "searchweb":
					if (!string.IsNullOrEmpty(arguments))
					{
						await _searchModule.SearchWebAsync(message, channel, arguments);
					}
					else
					{
						await channel.SendMessageAsync("Por favor, forneça um título para a busca. Ex: `!searchWeb Nome do Titulo`");
					}
					break;
				case "search":
					if (!string.IsNullOrEmpty(arguments))
					{
						await _searchModule.SearchOnScrollTracker(message, channel, arguments);
					}
					else
					{
						await channel.SendMessageAsync("Por favor, forneça um título para a busca. Ex: `!search Nome do Titulo`");
					}
					break;
				case "add":
					if (!string.IsNullOrEmpty(arguments))
					{
						await _searchModule.AddObraAsync(message, channel, arguments);
					}
					else
					{
						await channel.SendMessageAsync("Por favor, forneça um título para o cadastro. Ex: `!add Nome do Titulo`");
					}
					break;
				case "delete":
					if (!string.IsNullOrEmpty(arguments))
					{
						await _searchModule.AddObraAsync(message, channel, arguments);
					}
					else
					{
						await channel.SendMessageAsync("Por favor, forneça um título. Ex: `!delete Nome do Titulo`");
					}
					break;
					// Adicione outros comandos aqui...
			}
		}
	}
}
