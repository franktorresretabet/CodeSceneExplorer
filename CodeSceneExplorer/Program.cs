using CodeSceneExplorer.Application.Reporting;
using CodeSceneExplorer.Infrastructure.CodeScene;

var options = CodeSceneApiOptions.FromEnvironment();

using var httpClient = new HttpClient
{
    BaseAddress = options.BaseAddress
};

var api = new CodeSceneApiClient(httpClient, options);
var source = new MonthlyCodeHealthSource(api);
var useCase = new MonthlyCodeHealthReportUseCase(
    source,
    new MonthlyPeriodGenerator(),
    new MonthlyCodeHealthAggregator());
var runner = new MonthlyCodeHealthReportRunner(useCase, new MonthlyCodeHealthReportFormatter());
using var cancellationTokenSource = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellationTokenSource.Cancel();
};

var progress = new Progress<string>(message => Console.WriteLine(message));

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
