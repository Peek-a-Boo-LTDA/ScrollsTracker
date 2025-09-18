using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using ScrollsTracker.DiscordBot.Canais;
using ScrollsTracker.DiscordBot.Model;
using ScrollsTracker.DiscordBot.Model.Interfaces;

namespace ScrollsTracker.DiscordBot.Services
{
	public class KafkaConsumerService : BackgroundService
	{
		private readonly string _topic = "obras";
		private readonly IConsumer<string, string> _consumer;
		private readonly BotService _botService;

		public KafkaConsumerService(BotService botService)
		{
			var config = new ConsumerConfig
			{
				BootstrapServers = "localhost:9092",
				GroupId = "obras-consumer-group",
				AutoOffsetReset = AutoOffsetReset.Earliest
			};

			_consumer = new ConsumerBuilder<string, string>(config).Build();
			_consumer.Subscribe(_topic);
			_botService = botService;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			//TODO: bandAid para esperar o servico do discord subir, arrumar pra usar Channel ou qlq outra coisa
			await Task.Delay(3000, stoppingToken);

			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					var cr = _consumer.Consume(stoppingToken);
					var obra = JsonConvert.DeserializeObject<Obra>(cr.Message.Value);

					Console.WriteLine($"Recebida obra: {obra?.Titulo}");
					if (obra != null)
						_botService.SendObraMessage(obra).GetAwaiter().GetResult();
						//await ObraChannel.ChObra.Writer.WriteAsync(obra!, stoppingToken);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Erro no consumidor: {ex.Message}");
				}

				await Task.Delay(100, stoppingToken);
			}
		}

		public override void Dispose()
		{
			_consumer.Close();
			_consumer.Dispose();
			base.Dispose();
		}
	}
}
