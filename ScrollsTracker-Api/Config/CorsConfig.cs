namespace ScrollsTracker.Api.Config
{
    public static class CorsConfig
    {
        public static void AddCorsConfig(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("PermitirFrontend",
                    policy =>
                    {
                        //Url do front
                        policy.WithOrigins("http://localhost:5173")
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                    });
            });
        }
    }
}
