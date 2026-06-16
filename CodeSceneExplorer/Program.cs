using CodeSceneExplorer.Application.Abstractions;
using CodeSceneExplorer.Application.Reporting;
using CodeSceneExplorer.Infrastructure.CodeScene;
using CodeSceneExplorer.Infrastructure.Logging;
using log4net.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

internal class Program
{
    private static async Task Main(string[] args)
    {
        XmlConfigurator.ConfigureAndWatch(new FileInfo(Path.Combine(AppContext.BaseDirectory, "log4net.config")));

        HostApplicationBuilder builder = InitializeHostAppBuilder(args);

        using var host = builder.Build();
        using var cancellationTokenSource = new CancellationTokenSource();

        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cancellationTokenSource.Cancel();
        };

        var runner = host.Services.GetRequiredService<MonthlyCodeHealthReportRunner>();
        var progress = new Progress<string>(Console.WriteLine);

        try
        {
            var report = await runner.Build(
                DateOnly.FromDateTime(DateTime.UtcNow),
                progress,
                cancellationTokenSource.Token);

            Console.WriteLine(report);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Cancelled.");
        }
    }

    private static HostApplicationBuilder InitializeHostAppBuilder(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(new Log4NetLoggerProvider());

        builder.Services.Configure<ReportOptions>(builder.Configuration.GetSection("Report"));
        builder.Services.AddSingleton(CodeSceneApiOptions.FromEnvironment());
        builder.Services.AddSingleton(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<CodeSceneApiOptions>();
            return new HttpClient
            {
                BaseAddress = options.BaseAddress
            };
        });
        builder.Services.AddSingleton<ICodeSceneApi, CodeSceneApiClient>();
        builder.Services.AddSingleton<IMonthlyCodeHealthSource, MonthlyCodeHealthSource>();
        builder.Services.AddSingleton<MonthlyPeriodGenerator>();
        builder.Services.AddSingleton<MonthlyCodeHealthAggregator>();
        builder.Services.AddSingleton<IMonthlyCodeHealthReportUseCase, MonthlyCodeHealthReportUseCase>();
        builder.Services.AddSingleton<MonthlyCodeHealthReportFormatter>();
        builder.Services.AddSingleton<MonthlyCodeHealthReportRunner>();
        return builder;
    }
}