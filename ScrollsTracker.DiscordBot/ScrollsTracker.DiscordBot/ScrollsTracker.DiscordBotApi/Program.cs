using Discord;
using Discord.WebSocket;
using ScrollsTracker.DiscordBot.Infra.HttpService;
using ScrollsTracker.DiscordBot.Model.Interfaces;
using ScrollsTracker.DiscordBot.Services;
using ScrollsTracker.DiscordBot.Settings;
using ScrollsTracker.DiscordBotApi.HostedServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<IScrollsTrackerHttpService, ScrollsTrackerHttpService>();
builder.Services.Configure<DiscordSettings>(builder.Configuration.GetSection("Discord"));
builder.Services.AddSingleton(serviceProvider =>
{
	var config = new DiscordSocketConfig
	{
		GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
	};
	return new DiscordSocketClient(config);
});

// Registra o BotService
builder.Services.AddSingleton<IBotService, BotService>();
builder.Services.AddHostedService<DiscordBotHostedService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
