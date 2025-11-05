using Microsoft.EntityFrameworkCore;
using ScrollsTracker.Api.Repository;
using ScrollsTracker.Domain.Interfaces.Repository;
using ScrollsTracker.Infra.Repository.Context;

namespace ScrollsTracker.Api.Config
{
    public static class RepositoryConfig
    {
        public static void AddConfigRepository(this IServiceCollection services, string connectionString, string provider)
        {
			var assemblyName = typeof(AppDbContext).Assembly.GetName().Name;
			services.AddDbContext<AppDbContext>(options =>
			{
				switch (provider)
				{
					case "Sqlite":
						options.UseSqlite(connectionString,
							b => b.MigrationsAssembly(assemblyName));
						break;

					case "SqlServer":
						options.UseSqlServer(connectionString,
							b => b.MigrationsAssembly(assemblyName));
						break;
				}
			});

			services.AddScoped<IObraRepository, ObraRepository>();
		}
    }
}
