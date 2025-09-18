using Discord;
using Discord.WebSocket;
using ScrollsTracker.DiscordBot.Command;
using ScrollsTracker.DiscordBot.Infra.HttpService;
using ScrollsTracker.DiscordBot.Model.Interfaces;
using ScrollsTracker.DiscordBot.Modules;
using ScrollsTracker.DiscordBot.Services;
using ScrollsTracker.DiscordBot.Settings;

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
builder.Services.AddSingleton<BotService>();
builder.Services.AddSingleton<CommandHandler>();
builder.Services.AddHostedService<DiscordBotHostedService>();
builder.Services.AddHostedService<KafkaConsumerService>();
builder.Services.AddHttpClient<IScrollsTrackerHttpService, ScrollsTrackerHttpService>();
builder.Services.AddSingleton<ObrasModule>();

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
