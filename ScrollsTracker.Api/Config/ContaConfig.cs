using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScrollsTracker.Infra.Repository.Context;

namespace ScrollsTracker.Api.Config;

public static class ContaConfig
{
    public const string Administrador = "Administrador";
    public const string PodeCadastrarObras = "PodeCadastrarObras";
    public const string PodeGerenciarObras = "PodeGerenciarObras";
    public const string LimiteAcesso = "AcessoConta";

    public static void AddContas(this IServiceCollection services, IWebHostEnvironment ambiente)
    {
        var segurancaCookie = ambiente.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
        services.AddIdentity<IdentityUser, IdentityRole>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 8;
        }).AddEntityFrameworkStores<AppDbContext>();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = segurancaCookie;
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(PodeCadastrarObras, policy => policy.RequireAuthenticatedUser());
            options.AddPolicy(PodeGerenciarObras, policy => policy.RequireRole(Administrador));
        });
        services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
            options.Cookie.SecurePolicy = segurancaCookie;
        });
        services.Configure<MvcOptions>(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(LimiteAcesso, context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = 15, Window = TimeSpan.FromMinutes(1) }));
        });
    }

    public static async Task ConfigurarAdministradorAsync(IServiceProvider services, IConfiguration configuracao)
    {
        var email = configuracao["Administrador:Email"]?.Trim();
        if (string.IsNullOrWhiteSpace(email))
            return;

        var db = services.GetRequiredService<AppDbContext>();
        var usuarios = services.GetRequiredService<UserManager<IdentityUser>>();
        var papeis = services.GetRequiredService<RoleManager<IdentityRole>>();
        await using var transacao = await db.Database.BeginTransactionAsync();

        var administrador = await usuarios.FindByEmailAsync(email);
        if (administrador == null)
        {
            var senha = configuracao["Administrador:Senha"];
            if (string.IsNullOrEmpty(senha))
                throw new InvalidOperationException("Configure Administrador:Senha para criar a conta inicial.");

            administrador = new IdentityUser { UserName = email, Email = email };
            Verificar(await usuarios.CreateAsync(administrador, senha));
            if (!await papeis.RoleExistsAsync(Administrador))
                Verificar(await papeis.CreateAsync(new IdentityRole(Administrador)));
            Verificar(await usuarios.AddToRoleAsync(administrador, Administrador));
        }
        else if (!await usuarios.IsInRoleAsync(administrador, Administrador))
        {
            throw new InvalidOperationException("O e-mail do administrador já pertence a uma conta comum.");
        }

        var obrasLegadas = db.Obras.Where(obra =>
            EF.Property<string?>(obra, "UltimoCapituloLido") != null ||
            EF.Property<string?>(obra, "StatusLeitura") != null);
        var leituras = await obrasLegadas
            .Where(obra => !db.UsuarioObras.Any(leitura =>
                leitura.UsuarioId == administrador.Id && leitura.ObraId == obra.Id))
            .Select(obra => new UsuarioObra
            {
                ObraId = obra.Id,
                UsuarioId = administrador.Id,
                UltimoCapituloLido = EF.Property<string?>(obra, "UltimoCapituloLido"),
                StatusLeitura = EF.Property<string?>(obra, "StatusLeitura")
            })
            .ToListAsync();

        db.UsuarioObras.AddRange(leituras);
        await db.SaveChangesAsync();
        await obrasLegadas.ExecuteUpdateAsync(setters => setters
            .SetProperty(obra => EF.Property<string?>(obra, "UltimoCapituloLido"), (string?)null)
            .SetProperty(obra => EF.Property<string?>(obra, "StatusLeitura"), (string?)null));
        await transacao.CommitAsync();
    }

    private static void Verificar(IdentityResult resultado)
    {
        if (!resultado.Succeeded)
            throw new InvalidOperationException(string.Join(" ", resultado.Errors.Select(erro => erro.Description)));
    }
}
