using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ScrollsTracker.Application.HostedServices;

public sealed class AtualizadorDeObrasJob(AtualizacaoDeObras atualizacao, ILogger<AtualizadorDeObrasJob> logger)
	: BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await Task.Yield();
		using var timer = new PeriodicTimer(TimeSpan.FromHours(1));
		do
		{
			try
			{
				if (!await atualizacao.TentarExecutarAsync(stoppingToken))
					logger.LogInformation("Já há uma rodada de atualização de obras em execução.");
			}
			catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
			{
				return;
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Falha na rodada agendada de atualização de obras.");
			}
		}
		while (await timer.WaitForNextTickAsync(stoppingToken));
	}
}
