using Microsoft.Extensions.Options;

namespace CodeSceneExplorer.Application.Reporting;

public sealed class MonthlyCodeHealthReportRunner(
    IMonthlyCodeHealthReportUseCase useCase,
    MonthlyCodeHealthReportFormatter formatter,
    IOptions<ReportOptions> options)
{
    public async Task<string> Build(
        DateOnly today,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var start = options.Value.GetStartDate() ?? new DateOnly(today.Year - 1, 9, 1);
        var report = await useCase.Build(start, today, progress, cancellationToken).ConfigureAwait(false);
        return formatter.Format(report);
    }
}
