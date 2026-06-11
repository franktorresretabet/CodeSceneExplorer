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

var report = await runner.Build(DateOnly.FromDateTime(DateTime.UtcNow));
Console.WriteLine(report);
