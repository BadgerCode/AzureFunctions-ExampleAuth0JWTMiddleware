using ExampleAuthMiddleware.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ExampleAuthMiddleware
{
    public static class DependencyInjection
    {
        public static void InjectServices(IConfiguration configuration, IServiceCollection services)
        {
            services.AddHttpClient("Auth0", c => c.BaseAddress = new Uri(configuration["Auth0BaseURL"]));
            services.AddTransient<UserService>();

            // configuration["RatingsStorageConnectionString"]
        }
    }
}
