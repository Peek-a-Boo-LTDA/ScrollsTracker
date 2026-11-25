using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ScrollsTracker.Application.Commands;
using ScrollsTracker.Domain.Interfaces;
using ScrollsTracker.Domain.Interfaces.Repository;

namespace ScrollsTracker.Application.Handlers;

public sealed class AtualizarObrasCommandHandler(
	IObraRepository repositorio,
	IServiceScopeFactory scopes,
	ILogger<AtualizarObrasCommandHandler> logger) : IRequestHandler<AtualizarObrasCommand>
{
	public async Task Handle(AtualizarObrasCommand request, CancellationToken cancellationToken)
	{
		var tempo = Stopwatch.StartNew();
		var ids = await repositorio.ObterIdsParaAtualizarAsync(cancellationToken);
		var atualizadas = 0;
		var falhas = 0;
		using var escrita = new SemaphoreSlim(1);

		await Parallel.ForEachAsync(ids, new ParallelOptions
		{
			MaxDegreeOfParallelism = 3,
			CancellationToken = cancellationToken
		}, async (id, token) =>
		{
			try
			{
				await using var scope = scopes.CreateAsyncScope();
				var servicos = scope.ServiceProvider;
				var repo = servicos.GetRequiredService<IObraRepository>();
				var obra = await repo.GetObraByIdAsync(id, token);
				if (obra is null)
					return;

				var resultado = await servicos.GetRequiredService<IObraAggregatorService>()
					.BuscarEAtualizaObraAsync(obra, token);

				await escrita.WaitAsync(token);
				try
				{
					if (await repo.SalvarAtualizacaoAsync(resultado, token))
						Interlocked.Increment(ref atualizadas);
					else
					{
						Interlocked.Increment(ref falhas);
						logger.LogWarning("A obra {ObraId} foi alterada durante a consulta e ficou para a próxima rodada.", id);
					}
				}
				finally
				{
					escrita.Release();
				}
			}
			catch (Exception ex) when (!token.IsCancellationRequested)
			{
				Interlocked.Increment(ref falhas);
				logger.LogError(ex, "Falha ao atualizar a obra {ObraId}.", id);
			}
		});

		logger.LogInformation("Rodada concluída em {DuracaoMs} ms. Obras elegíveis: {Elegiveis}. Atualizadas: {Atualizadas}. Falhas: {Falhas}.",
			tempo.ElapsedMilliseconds, ids.Count, atualizadas, falhas);
	}
}
