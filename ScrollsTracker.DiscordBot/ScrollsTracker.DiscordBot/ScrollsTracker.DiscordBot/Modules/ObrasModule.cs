using Discord;
using Discord.WebSocket;
using ScrollsTracker.DiscordBot.Model.Interfaces;

namespace ScrollsTracker.DiscordBot.Modules
{
	public class ObrasModule
	{
		private IScrollsTrackerHttpService _scrollsTrackerHttpService;

		public ObrasModule(IScrollsTrackerHttpService scrollsTrackerHttpService)
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

		public async Task AddObraAsync(SocketMessage message, IMessageChannel? channel, string titulo)
		{
			var result = await _scrollsTrackerHttpService.CadastrarObraAsync(titulo);

			if (channel != null)
			{
				if (!result)
				{
					await channel.SendMessageAsync("Não foi possível cadastrar a obra. Talvez ela já exista no banco de dados.");
					return;
				}
				
				await channel.SendMessageAsync("Obra cadastrada com sucesso!");
			}
		}

		public async Task DeletarObraAsync(SocketMessage message, IMessageChannel? channel, string titulo)
		{
			var result = await _scrollsTrackerHttpService.DeletarObraAsync(titulo);

			if (channel != null)
			{
				if (!result)
				{
					await channel.SendMessageAsync("Não foi possível deletar a obra.");
					return;
				}

				await channel.SendMessageAsync("Obra deletada com sucesso!");
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
