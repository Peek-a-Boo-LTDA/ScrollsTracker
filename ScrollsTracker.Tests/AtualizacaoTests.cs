using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ScrollsTracker.Application.Commands;
using ScrollsTracker.Application.HostedServices;

namespace ScrollsTracker.Tests;

public class AtualizacaoTests
{
	[Fact]
	public async Task RodadaEmExecucaoImpedeSegundaRodada()
	{
		var handler = new RodadaControlada();
		var servicos = new ServiceCollection();
		servicos.AddLogging();
		servicos.AddMediatR(config => config.RegisterServicesFromAssemblyContaining<AtualizarObrasCommand>());
		servicos.AddSingleton<IRequestHandler<AtualizarObrasCommand>>(handler);
		await using var provider = servicos.BuildServiceProvider();
		var coordenador = new AtualizacaoDeObras(provider.GetRequiredService<IServiceScopeFactory>());

		var rodada = coordenador.TentarExecutarAsync(CancellationToken.None);
		try
		{
			if (await Task.WhenAny(handler.Comecou.Task, rodada).WaitAsync(TimeSpan.FromSeconds(5)) == rodada)
				throw new InvalidOperationException($"A rodada terminou antes de entrar no handler: {await rodada}.");
			await handler.Comecou.Task.WaitAsync(TimeSpan.FromSeconds(5));
			Assert.False(await coordenador.TentarExecutarAsync(CancellationToken.None));
		}
		finally
		{
			handler.Terminar.SetResult();
		}

		Assert.True(await rodada.WaitAsync(TimeSpan.FromSeconds(5)));
		Assert.True(await coordenador.TentarExecutarAsync(CancellationToken.None));
	}

	private sealed class RodadaControlada : IRequestHandler<AtualizarObrasCommand>
	{
		public TaskCompletionSource Comecou { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
		public TaskCompletionSource Terminar { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

		public async Task Handle(AtualizarObrasCommand request, CancellationToken cancellationToken)
		{
			Comecou.TrySetResult();
			await Terminar.Task.WaitAsync(cancellationToken);
		}
	}
}
