using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ScrollsTracker.Application.Commands;

namespace ScrollsTracker.Application.HostedServices;

public sealed class AtualizacaoDeObras(IServiceScopeFactory scopes)
{
	private int _emExecucao;

	public async Task<bool> TentarExecutarAsync(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (Interlocked.CompareExchange(ref _emExecucao, 1, 0) != 0)
			return false;

		try
		{
			await using var scope = scopes.CreateAsyncScope();
			await scope.ServiceProvider.GetRequiredService<ISender>()
				.Send(new AtualizarObrasCommand(), cancellationToken);
			return true;
		}
		finally
		{
			Volatile.Write(ref _emExecucao, 0);
		}
	}
}
