using ScrollsTracker.Application.Services;
using ScrollsTracker.Domain.Interfaces;
using ScrollsTracker.Infra.Sources;
using System.Net.Http.Headers;

namespace ScrollsTracker.Api.Config
{
    public static class ServiceConfig
    {
        public static void AddConfigService(this IServiceCollection services)
        {
			AdicionarFonte<MangaUpdateSource>(services);
			AdicionarFonte<MangaDexSource>(services);
			AdicionarFonte<MangaKatanaSource>(services);
			AdicionarFonte<MangaHereSource>(services);
			AdicionarFonte<KitsuSource>(services, "application/vnd.api+json");
			services.AddSingleton<CacheCandidatos>();
			services.AddScoped<IObraAggregatorService, ObraAggregatorService>();
		}

		private static void AdicionarFonte<TFonte>(IServiceCollection services, string? accept = null)
			where TFonte : class, IObraSource
		{
			var limite = new SemaphoreSlim(3);
			services.AddHttpClient<IObraSource, TFonte>(typeof(TFonte).Name, cliente =>
			{
				cliente.Timeout = TimeSpan.FromSeconds(6);
				cliente.DefaultRequestHeaders.UserAgent.ParseAdd("ScrollsTracker/1.0 (localhost)");
				if (accept is not null)
					cliente.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(accept));
			}).AddHttpMessageHandler(() => new LimiteDaFonte(limite));
		}

		private sealed class LimiteDaFonte(SemaphoreSlim limite) : DelegatingHandler
		{
			protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				await limite.WaitAsync(cancellationToken);
				try
				{
					return await base.SendAsync(request, cancellationToken);
				}
				finally
				{
					limite.Release();
				}
			}
		}
    }
}
