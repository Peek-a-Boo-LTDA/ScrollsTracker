using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using ScrollsTracker.Api.AutoMapperProfile;
using ScrollsTracker.Api.Config;
using ScrollsTracker.Application.Reference;
using ScrollsTracker.Infra.Repository.Context;

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
builder.Services.AddControllers();
builder.Services.AddConfigService();
builder.Services.AddConfigFacade();
builder.Services.AddConfigJob();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

builder.Services.AddMediatR(cfg => {
	cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
	cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyReference).Assembly);
});

var app = builder.Build();

//using (var scope = app.Services.CreateScope())
//{
//	var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//	db.Database.Migrate();
//}

app.UseCors("PermitirFrontend");

//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();


// Serve arquivos estáticos (React build)
app.UseDefaultFiles();   // Procura index.html por padrão
app.UseStaticFiles();    // Serve arquivos de wwwroot

// Fallback para index.html (rotas SPA)
app.MapFallbackToFile("index.html");

app.Run();
