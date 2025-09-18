using ScrollsTracker.Domain.Models;

namespace ScrollsTracker.Domain.Interfaces
{
	public interface IKafkaProducerService
	{
		Task ProduceAsync(Obra obra);
	}
}
