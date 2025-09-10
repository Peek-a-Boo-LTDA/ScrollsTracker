using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using ScrollsTracker.DiscordBot.Command;
using ScrollsTracker.DiscordBot.Model;
using ScrollsTracker.DiscordBot.Model.Interfaces;
using ScrollsTracker.DiscordBot.Settings;

namespace ScrollsTracker.DiscordBot.Services
{
	public class BotService : IBotService
	{
		private readonly DiscordSocketClient _client;
		private readonly CommandHandler _commandHandler;
		private IMessageChannel? channel;

		private ulong _fixedChannelId;
		private string _token;

		public BotService(DiscordSocketClient client, IOptions<DiscordSettings> discordSettingsOptions, IScrollsTrackerHttpService scrollsTrackerHttpService)
		{
			_client = client;
			var discordSettings = discordSettingsOptions.Value;

			_fixedChannelId = discordSettings.FixedChannelId;
			_token = discordSettings.Token ?? throw new Exception("Token não configurado!");

			_commandHandler = new CommandHandler(_client, _fixedChannelId, scrollsTrackerHttpService);
		}

		public async Task StartAsync()
		{
			_client.Log += LogAsync;
			_client.Ready += OnReadyAsync;

			await _client.LoginAsync(TokenType.Bot, _token);
			await _client.StartAsync();

			await _commandHandler.InitializeAsync();
		}

		public async Task StopAsync()
		{
			await _client.StopAsync();
		}

		private Task LogAsync(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}

		public async Task SendMessageThroughChannel(string message)
		{
			channel = _client.GetChannel(_fixedChannelId) as IMessageChannel;
			if (channel != null)
			{
				await channel.SendMessageAsync(message);
			}
		}

		public async Task SendComplexMessage(string title, string description, string thumbnailUrl)
		{
			channel = _client.GetChannel(_fixedChannelId) as IMessageChannel;
			if (channel == null)
			{
				return;
			}

			var embed = new EmbedBuilder
			{
				Title = "Título da sua mensagem",
				Description = "Descrição...",
			};

			embed.WithImageUrl(thumbnailUrl);
			//embed.WithThumbnailUrl(thumbnailUrl);

			await channel.SendMessageAsync(embed: embed.Build());
		}

		public async Task SendObraMessage(Obra obra)
		{
			channel = _client.GetChannel(_fixedChannelId) as IMessageChannel;
			if (channel == null)
			{
				return;
			}

			var embed = new EmbedBuilder
			{
				Title = "Capitulo Novo! \n\n" + obra.Titulo,
				Description = $"{obra.Descricao} \n\n Novo Capitulo: {obra.TotalCapitulos}",
			};

			embed.WithImageUrl(obra.Imagem);
			embed.Color = Color.DarkPurple;

			await channel.SendMessageAsync(embed: embed.Build());
		}

		private async Task OnReadyAsync()
		{
			Console.WriteLine($"Bot conectado como {_client.CurrentUser}");
			await SendMessageThroughChannel("Estou online no canal fixo! 🚀");
		}
	}
}
