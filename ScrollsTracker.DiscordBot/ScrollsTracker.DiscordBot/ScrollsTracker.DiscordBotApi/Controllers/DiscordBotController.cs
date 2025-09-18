using Microsoft.AspNetCore.Mvc;
using ScrollsTracker.DiscordBot.Model.Interfaces;
using ScrollsTracker.DiscordBot.Services;

namespace ScrollsTracker.DiscordBotApi.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class DiscordBotController : ControllerBase
	{
		private readonly ILogger<DiscordBotController> _logger;
		private readonly BotService _botService;

		public DiscordBotController(ILogger<DiscordBotController> logger, BotService botService)
		{
			_logger = logger;
			_botService = botService;
		}

		[HttpGet(Name = "SendMessage")]
		public async Task<IActionResult> GetAsync()
		{
			await _botService.SendEmbedMessageAsync("Teste de mensagem do bot via API", "Descricao", "https://mangadex.org/covers/5a3cdf57-ec79-4baa-ada6-31aa2d8bb3a9/80f3c90c-ac05-4d37-bc1a-95633e06d359.jpg");
			return Ok();
		}
	}
}
