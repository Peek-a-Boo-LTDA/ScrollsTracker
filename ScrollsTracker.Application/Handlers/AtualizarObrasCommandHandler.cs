using Microsoft.Extensions.Logging;
using ScrollsTracker.Domain.Interfaces.Repository;
using ScrollsTracker.Domain.Interfaces;
using MediatR;
using ScrollsTracker.Application.Commands;
using ScrollsTracker.Domain.Models;

namespace ScrollsTracker.Application.Handlers
{
	public class AtualizarObrasCommandHandler : IRequestHandler<AtualizarObrasCommand>
	{
		private readonly IObraRepository _obraRepository;
		private readonly IObraAggregatorService _aggregatorService;
		private readonly ILogger<AtualizarObrasCommandHandler> _logger;
		private readonly IKafkaProducerService _producer;

		public AtualizarObrasCommandHandler(IObraRepository obraRepository, IObraAggregatorService aggregatorService, ILogger<AtualizarObrasCommandHandler> logger, IKafkaProducerService producer)
		{
			_obraRepository = obraRepository;
			_aggregatorService = aggregatorService;
			_logger = logger;
			_producer = producer;
		}

		public async Task Handle(AtualizarObrasCommand request, CancellationToken cancellationToken)
		{
			//TODO: Fazer o select de apenas obras que ainda lançam
			var obras = await _obraRepository.ObterTodasObrasParaAtualizarAsync();

			if (obras == null || !obras.Any())
			{
				_logger.LogWarning("Nenhuma obra encontrada para atualização.");
				return;
			}

			_logger.LogInformation("Iniciando atualização de obras...");

			//TODO: Esse método provavelmente vai dar problema no futuro caso tenha muitas obras.
			foreach (Obra obra in obras) 
			{
				var atualizacaoResult = await _aggregatorService.BuscarEAtualizaObraAsync(obra);
				var result = await _obraRepository.UpdateObraAsync(atualizacaoResult.Obra);

				if(result >= 1) 
				{
					if (atualizacaoResult.NovoCapitulo)
						await _producer.ProduceAsync(atualizacaoResult.Obra);

					_logger.LogInformation($"Obra {atualizacaoResult.Obra.Titulo} atualizada com sucesso. ID: {atualizacaoResult.Obra.Id}");
				}
				else
				{
					_logger.LogWarning($"Falha ao atualizar a obra {atualizacaoResult.Obra.Titulo}. ID: {atualizacaoResult.Obra.Id}");
				}
			}
		}
	}
}
