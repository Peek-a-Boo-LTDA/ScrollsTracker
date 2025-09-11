using Discord;
using Discord.WebSocket;
using ScrollsTracker.DiscordBot.Model.Interfaces;

namespace ScrollsTracker.DiscordBot.Modules
{
	public class SearchModule
	{
		private IScrollsTrackerHttpService _scrollsTrackerHttpService;

		public SearchModule(IScrollsTrackerHttpService scrollsTrackerHttpService)
		{
			_scrollsTrackerHttpService = scrollsTrackerHttpService;
		}

		public async Task SearchWebAsync(SocketMessage message, IMessageChannel? channel, string titulo)
		{
			var obra = await _scrollsTrackerHttpService.ProcurarObraNasApisExternasAsync(titulo);

			if (channel != null)
			{
				var embed = new EmbedBuilder
				{
					Title = obra.Titulo,
					Description = $"{obra.Descricao} \n\n Total de Capitulos: {obra.TotalCapitulos}",
				};

				embed.WithImageUrl(obra.Imagem);
				embed.Color = Color.DarkPurple;

				await channel.SendMessageAsync(embed: embed.Build());
			}
		}

		public async Task SearchOnScrollTracker(SocketMessage message, IMessageChannel? channel, string titulo)
		{
			var obras = await _scrollsTrackerHttpService.ProcurarObraNoScrollTrackerAsync(titulo);

			foreach(var obra in obras)
			{
				 if (channel != null)
				{
					var embed = new EmbedBuilder
					{
						Title = obra.Titulo,
						Description = $"{obra.Descricao} \n\n Total de Capitulos: {obra.TotalCapitulos}",
					};

					embed.WithImageUrl(obra.Imagem);
					embed.Color = Color.DarkPurple;

					await channel.SendMessageAsync(embed: embed.Build());
				}
			}
		}
	}
}
