using Microsoft.Extensions.Hosting;
using ScrollsTracker.DiscordBot.Command;
using ScrollsTracker.DiscordBot.Model.Interfaces;

namespace ScrollsTracker.DiscordBot.Services
{
	public class DiscordBotHostedService : IHostedService
	{
		//TODO: Toda estrutura o bot do discord ta horrivel, arrumar
		private readonly BotService _botService;
		private readonly CommandHandler _commandHandler;

		public DiscordBotHostedService(BotService botService, CommandHandler commandHandler)
		{
			_botService = botService;
			_commandHandler = commandHandler;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			await _botService.StartAsync();
			await _commandHandler.InitializeAsync(_botService);
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			await _botService.StopAsync();
		}
	}
}
