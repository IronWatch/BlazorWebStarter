using BlazorWebStarter.Helpers;

namespace BlazorWebStarter.Services;

public partial class AppEnvConfiguration
    : DotEnvConfiguration<AppEnvConfiguration>
{
    public AppEnvConfiguration() : base() { }

    [EnvList("WEB_LISTEN_ADDRESSES", defaultValue: "0.0.0.0")]
    public List<string> WebListenAddresses { get; private set; } = null!;

    [Env("WEB_HOSTNAME", optional: true)]
    public string? WebHostname { get; private set; }

    [Env<int>("WEB_HTTP_PORT", defaultValue: "80")]
    public int WebHttpPort { get; private set; }

    [Env("DATABASE_HOSTNAME")]
    public string DatabaseHostname { get; set; } = null!;

    [Env<int>("DATABASE_PORT", defaultValue: "5432")]
    public int DatabasePort { get; set; }

    [Env("DATABASE_DBNAME")]
    public string DatabaseDbName { get; set; } = null!;

    [Env("DATABASE_USERNAME")]
    public string DatabaseUsername { get; set; } = null!;

    [Env("DATABASE_PASSWORD")]
    public string DatabasePassword { get; set; } = null!;
}
