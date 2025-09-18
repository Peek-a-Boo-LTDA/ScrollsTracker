using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using ScrollsTracker.DiscordBot.Command;
using ScrollsTracker.DiscordBot.Model;
using ScrollsTracker.DiscordBot.Model.Interfaces;
using ScrollsTracker.DiscordBot.Settings;

namespace ScrollsTracker.DiscordBot.Services
{
	public class BotService
	{
		public DiscordSocketClient Client { get; private set; }
		//private readonly CommandHandler _commandHandler;
		public IMessageChannel? channel { get; private set; }

		public ulong FixedChannelId;
		private string _token;

		public BotService(DiscordSocketClient client, IOptions<DiscordSettings> discordSettingsOptions, IScrollsTrackerHttpService scrollsTrackerHttpService)
		{
			this.Client = client;
			var discordSettings = discordSettingsOptions.Value;

			if (discordSettings.FixedChannelId == 0)
			{
				throw new Exception("FixedChannelId não configurado!");
			}
			FixedChannelId = discordSettings.FixedChannelId;

			_token = discordSettings.Token ?? throw new Exception("Token não configurado!");

			//_commandHandler = new CommandHandler(this.Client, _fixedChannelId, scrollsTrackerHttpService);
		}

		public async Task StartAsync()
		{
			Client.Log += LogAsync;
			Client.Ready += OnReadyAsync;

			await Client.LoginAsync(TokenType.Bot, _token);
			await Client.StartAsync();
		}

		public async Task StopAsync()
		{
			await Client.StopAsync();
		}

		private Task LogAsync(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}

		public async Task SendMessageThroughChannel(string message)
		{
			channel = Client.GetChannel(FixedChannelId) as IMessageChannel;
			if (channel != null)
			{
				await channel.SendMessageAsync(message);
			}
		}

		public async Task SendEmbedMessageAsync(string title, string description, string thumbnailUrl)
		{
			channel = Client.GetChannel(FixedChannelId) as IMessageChannel;
			if (channel == null)
			{
				return;
			}

			var embed = new EmbedBuilder
			{
				Title = title,
				Description = description,
			};

			embed.WithImageUrl(thumbnailUrl);
			//embed.WithThumbnailUrl(thumbnailUrl);

			await channel.SendMessageAsync(embed: embed.Build());
		}

		public async Task SendEmbedMessageAsync(EmbedBuilder embedMessage)
		{
			await channel!.SendMessageAsync(embed: embedMessage.Build());
		}

		public async Task SendObraMessage(Obra obra)
		{
			channel = Client.GetChannel(FixedChannelId) as IMessageChannel;
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
			Console.WriteLine($"Bot conectado como {Client.CurrentUser}");
			await SendMessageThroughChannel("Estou online no canal fixo! 🚀");
		}
	}
}
