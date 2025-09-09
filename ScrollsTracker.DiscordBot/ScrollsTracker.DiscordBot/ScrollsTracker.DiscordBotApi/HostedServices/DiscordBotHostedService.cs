using ScrollsTracker.DiscordBot.Model.Interfaces;
using ScrollsTracker.DiscordBot.Services;

namespace ScrollsTracker.DiscordBotApi.HostedServices
{
	public class DiscordBotHostedService : IHostedService
	{
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
