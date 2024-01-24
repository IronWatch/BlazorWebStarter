using System.Net;
using Microsoft.EntityFrameworkCore;
using BlazorWebStarter.Services;
using BlazorWebStarter.WebComponents;
using BlazorWebStarter.Database;

namespace BlazorWebStarter.Daemons;

public class WebDaemon(
    [FromKeyedServices(ServiceKeys.Args)] string[] args,
    AppEnvConfiguration configuration,
    ILogger<WebDaemon> logger,
    IServiceProvider rootSp) 
    : BaseDaemon<WebDaemon>(logger)
{
    protected override async Task EntryPoint(CancellationToken cancellationToken)
    {
        try
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            builder.WebHost.UseKestrel(kestrel =>
            {
                foreach (string address in configuration.WebListenAddresses)
                {
                    kestrel.Listen(IPAddress.Parse(address), configuration.WebHttpPort);
                }
            });

            builder.Services.Configure<ConsoleLifetimeOptions>(consoleLifetime =>
            {
                consoleLifetime.SuppressStatusMessages = true;
            });

            builder.Services.AddKeyedSingleton(ServiceKeys.RootSp, rootSp);

            builder.Services.AddSingleton(sp => sp
                .GetRequiredKeyedService<IServiceProvider>(ServiceKeys.RootSp)
                .GetRequiredService<AppEnvConfiguration>());

            builder.Services.AddSingleton(sp => sp
                .GetRequiredKeyedService<IServiceProvider>(ServiceKeys.RootSp)
                .GetRequiredService<IDbContextFactory<AppDbContext>>());

            builder.Services.AddTransient<DaemonInteractionService>();

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddHttpContextAccessor();

            WebApplication app = builder.Build();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            Logger.LogInformation("{daemon} started", nameof(WebDaemon));
            this.Running = true;

            await app.RunAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogCritical(ex, "{daemon} has crashed with a critical exception!", nameof(WebDaemon));
        }
    }
}
