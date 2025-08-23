using ScrollsTracker.Api.AutoMapperProfile;
using ScrollsTracker.Api.Config;
using ScrollsTracker.Application.Reference;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("ConnectionString do banco não encontrada");
}
builder.Services.AddConfigRepository(connectionString);

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

app.UseCors("PermitirFrontend");

app.UseHttpsRedirection();
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
