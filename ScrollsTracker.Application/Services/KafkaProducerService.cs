using Confluent.Kafka;
using ScrollsTracker.Domain.Interfaces;
using ScrollsTracker.Domain.Models;
using System.Text.Json;

namespace ScrollsTracker.Application.Services
{
	public class KafkaProducerService : IKafkaProducerService
	{
		private readonly IProducer<string, string> _producer;
		private readonly string _topic = "obras";

		public KafkaProducerService()
		{
			var config = new ProducerConfig
			{
				BootstrapServers = "localhost:9092" // endereço do Kafka
			};

			_producer = new ProducerBuilder<string, string>(config).Build();
		}

		public async Task ProduceAsync(Obra obra)
		{
			await _producer.ProduceAsync(_topic, new Message<string, string>
			{
				Key = obra.Id.ToString(),
				Value = JsonSerializer.Serialize(obra)
			});
		}
	}
}
