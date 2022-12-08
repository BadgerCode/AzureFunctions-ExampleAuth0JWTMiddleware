using ExampleAuthMiddleware;
using ExampleAuthMiddleware.Auth;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(workerApplication =>
    {
        workerApplication.UseMiddleware<JWTMiddleware>();
    })
    .ConfigureServices((ctx, services) => DependencyInjection.InjectServices(ctx.Configuration, services))
    .Build();

host.Run();
