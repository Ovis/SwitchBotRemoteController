using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SwitchBotRemoteController.Extensions;
using SwitchBotRemoteController.Options;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((hostContext, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddHttpClient();

        services.Configure<SwitchBotOption>(option =>
        {
            // 環境変数のTokenの値を取得
            option.Token = hostContext.Configuration["SwitchBotToken"] ?? string.Empty;
            option.Secret = hostContext.Configuration["SwitchBotSecret"] ?? string.Empty;

        });

        services.AddSwitchBotClient();
    })
    .Build();

host.Run();
