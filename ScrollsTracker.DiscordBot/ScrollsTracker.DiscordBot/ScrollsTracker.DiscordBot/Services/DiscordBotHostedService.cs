using Microsoft.Extensions.Hosting;
using ScrollsTracker.DiscordBot.Model.Interfaces;

namespace ScrollsTracker.DiscordBot.Services
{
	public class DiscordBotHostedService : IHostedService
	{
		//TODO: Toda estrutura o bot do discord ta horrivel, arrumar
		private readonly IBotService _botService;

		public DiscordBotHostedService(IBotService botService)
		{
			_botService = botService;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			await _botService.StartAsync();
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			await _botService.StopAsync();
		}
	}
}
