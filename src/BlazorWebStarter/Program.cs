using Microsoft.EntityFrameworkCore;
using BlazorWebStarter.Daemons;
using BlazorWebStarter.Database;
using BlazorWebStarter.Services;
using System.CommandLine;

// Option to load a DotEnv format file in Development
// In Production, this typically isn't used as the Environment variables are set by the container host system
Option<string> envOption = new("--env", "DotEnv formatted env file to load");
RootCommand rootCommand = [envOption];
rootCommand.TreatUnmatchedTokensAsErrors = false;

rootCommand.SetHandler(
    symbol: envOption, 
    handle: async (envOptionValue) =>
{
    /* ===========================================
     * Initial Steps Before Building the Root Host
     * ===========================================
    */
    
    // Initial logger
    ILogger initLogger = LoggerFactory.Create(l => l
        .SetMinimumLevel(LogLevel.Trace)
        .AddConsole())
        .CreateLogger<Program>();

    // Load DotEnv
    if (envOptionValue is not null)
    {
        initLogger.LogInformation("Loading environment variables from \"{path}\"", envOptionValue);

        // !!! REMOVE THIS LINE ONCE YOU HAVE COPIED "example.env" to ".env" !!!
        initLogger.LogCritical("If you haven't copied \"example.env\" to the file \"{path}\" the application is going to crash here! Remove this logger call from \"Program.cs\" Line 34 once you have done so!", envOptionValue);

        try
        {
            _ = await AppEnvConfiguration.LoadDotEnvAsync(envOptionValue);
        }
        catch (Exception ex)
        {
            initLogger.LogCritical(ex, "Failed to load environment variables");
            return;
        }
    }

    // EF Design Time Hotpath
    if (EF.IsDesignTime)
    {
        WebApplicationBuilder efDesignTimeBuilder = WebApplication.CreateBuilder(args);
        efDesignTimeBuilder.Services.AddSingleton(new AppEnvConfiguration());
        efDesignTimeBuilder.Services.AddDbContext<AppDbContext>();
        try
        {
            _ = efDesignTimeBuilder.Build();
        }
        catch (HostAbortedException) { } // EF intentionally aborts the host during design time.
        return;
    }

    /* ===========================================
     * Build Root Host
     * ===========================================
    */

    // Root Host builder (not a webhost builder, just for minimal service hosting)
    HostBuilder builder = new();

    // Logging used by the daemon's directly
    builder.ConfigureLogging(logging =>
    {
        logging
            .SetMinimumLevel(LogLevel.Information)
            .AddConsole();
    });

    // HostBuilder needs this explicitly set
    builder.ConfigureAppConfiguration(appConfiguration =>
    {
        appConfiguration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false);
    });

    // Define shared services, including background daemons here
    bool serviceSuccess = false;
    builder.ConfigureServices(services =>
    {
        // Remove the "Press Ctrl-C to exit" and other lifetime messages
        services.Configure<ConsoleLifetimeOptions>(consoleLifetime =>
        {
            consoleLifetime.SuppressStatusMessages = true;
        });

        // child hostbuilders in daemons will need the command line args themselves
        services.AddKeyedSingleton(ServiceKeys.Args, args);

        // Load the Environment Variables. This will generate errors if any required variables are missing
        try
        {
            services.AddSingleton(new AppEnvConfiguration());
        }
        catch (AggregateException ex)
        {
            foreach (Exception innerEx in  ex.InnerExceptions)
            {
                initLogger.LogCritical("Configuration Error: {error}", innerEx.Message);
            }
            return;
        }

        // Adding the DB Context factory
        services.AddDbContextFactory<AppDbContext>();

        // Add Daemon's here
        // For the Starter, only one is included, but other background services can easily be added here.
        services.AddHostedService<WebDaemon>();

        serviceSuccess = true;
    });

    IHost host = builder.Build();
    if (!serviceSuccess) return;

    /* ===========================================
     * Perform DB Migrations
     * ===========================================
    */

    try
    {
        initLogger.LogInformation("Applying any pending database migrations");
        await AppDbContext.AppStartup(host.Services);
    }
    catch (Exception ex)
    {
        initLogger.LogCritical(ex, "Initial connection to the Database failed!");
        return;
    }

    /* ===========================================
     * Run Host
     * ===========================================
    */

    await host.RunAsync();

});

// Actually run the root command
await rootCommand.InvokeAsync(args);
