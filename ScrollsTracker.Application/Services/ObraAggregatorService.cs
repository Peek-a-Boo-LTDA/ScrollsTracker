using Microsoft.Extensions.Logging;
using ScrollsTracker.Application.Services.Filter;
using ScrollsTracker.Domain.Interfaces;
using ScrollsTracker.Domain.Models;

namespace ScrollsTracker.Application.Services
{
	public class ObraAggregatorService : IObraAggregatorService
	{
        public readonly IEnumerable<IObraSource> _sources;
        private readonly ILogger<ObraAggregatorService> _logger;

		public ObraAggregatorService(IEnumerable<IObraSource> sources, ILogger<ObraAggregatorService> logger)
		{
			_sources = sources;
			_logger = logger;
		}

		public async Task<Obra> BuscarObraAgregadaAsync(string titulo)
		{
			var obraFilter = new ObraFilter(titulo);

			foreach (var source in _sources)
			{
				try
				{
					var searchResult = await source.ObterObraAsync(titulo);

					if (searchResult == null || searchResult.Obra is null || string.IsNullOrEmpty(searchResult.Obra.Titulo))
					{
						_logger.LogInformation($"Busca da fonte: {source.SourceName} retornou resultados vazios");
						continue;
					}

					if (searchResult.Score <= 40)
					{
						_logger.LogInformation($"Busca da fonte: {source.SourceName} retornou resultados com socre muito baixo");
						continue;
					}

					if (searchResult != null)
					{
						obraFilter.Filtrar(searchResult);
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"Falha ao buscar dados da fonte: {source.SourceName}");
				}
			}

			return obraFilter.ObraFiltrada;
		}

		//TODO: Esse método pode ser melhorado com o filtro ficando mais inteligente
		// Ele tmb poderia dizer se a obra foi atualizada ou n
		public async Task<AtualizacaoResult> BuscarEAtualizaObraAsync(Obra obra)
		{
			var obraFilter = new ObraFilter(obra);
			var novoCapLancado = 0;
			foreach (var source in _sources)
			{
				try
				{
					//TODO Aplicar o DRY aqui, ta zuado
					var searchResult = await source.ObterObraAsync(obra.Titulo!);

					if (searchResult == null || searchResult.Obra is null || string.IsNullOrEmpty(searchResult.Obra.Titulo))
					{
						_logger.LogInformation($"Busca da fonte: {source.SourceName} retornou resultados vazios");
						continue;
					}

					if (searchResult.Score <= 40)
					{
						_logger.LogInformation($"Busca da fonte: {source.SourceName} retornou resultados com socre muito baixo");
						continue;
					}

					if (searchResult != null)
					{
						novoCapLancado += obraFilter.Atualizar(searchResult);
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"Falha ao buscar dados da fonte: {source.SourceName}");
				}
			}

			
			obraFilter.ObraFiltrada.DataVerificacao = DateTime.Now;
			return new AtualizacaoResult(obraFilter.ObraFiltrada, (novoCapLancado >= 1));
			
		}
	}
}
