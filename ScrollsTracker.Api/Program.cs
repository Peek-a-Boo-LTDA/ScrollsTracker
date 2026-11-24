using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using ScrollsTracker.Api.AutoMapperProfile;
using ScrollsTracker.Api.Config;
using ScrollsTracker.Api.Services;
using ScrollsTracker.Application.Reference;
using ScrollsTracker.Infra.Repository.Context;


internal class Program
{
	private static async Task Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		var provider = builder.Configuration["DatabaseProvider"];
		string? connectionString;

		if (string.IsNullOrEmpty(provider))
		{
			throw new InvalidOperationException("Provider do banco não encontrada");
		}

		switch (provider)
		{
			case "Sqlite":
				connectionString = builder.Configuration.GetConnectionString("Sqlite");
				break;

			case "SqlServer":
				connectionString = builder.Configuration.GetConnectionString("SqlServer");
				break;

			default:
				throw new InvalidOperationException($"Database provider '{provider}' não é suportado.");
		}

		if (string.IsNullOrEmpty(connectionString))
		{
			throw new InvalidOperationException("ConnectionString do banco não encontrada");
		}

		builder.Services.AddConfigRepository(connectionString, provider);

		builder.Services.AddLogging();
		builder.Services.AddCorsConfig();
		builder.Services.AddControllersWithViews();
		builder.Services.AddContas(builder.Environment);

		var diretorioChaves = builder.Configuration["DataProtection:KeysPath"];
		if (!string.IsNullOrWhiteSpace(diretorioChaves))
		{
			builder.Services.AddDataProtection()
				.SetApplicationName("ScrollsTracker")
				.PersistKeysToFileSystem(new DirectoryInfo(diretorioChaves));
		}
		builder.Services.AddScoped<LeituraService>();
		builder.Services.AddConfigService();
		builder.Services.AddConfigFacade();
		builder.Services.AddConfigJob();
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSwaggerGen();

		builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

		builder.Services.AddMediatR(cfg =>
		{
			cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
			cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyReference).Assembly);
		});

		var app = builder.Build();

		using (var scope = app.Services.CreateScope())
		{
			var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
			await db.Database.MigrateAsync();
			await ContaConfig.ConfigurarAdministradorAsync(scope.ServiceProvider, builder.Configuration);
		}

		app.UseCors("PermitirFrontend");

		app.UseHttpsRedirection();
		app.UseAuthentication();
		app.UseAuthorization();
		app.UseRateLimiter();
		app.MapControllers();
		app.UseSwagger();
		app.UseSwaggerUI();


		// Serve arquivos estáticos (React build)
		app.UseDefaultFiles();   // Procura index.html por padrão
		app.UseStaticFiles();    // Serve arquivos de wwwroot

		// Fallback para index.html (rotas SPA)
		app.MapFallbackToFile("index.html");

		await app.RunAsync();
	}
}