namespace CodeSceneExplorer.Application.Reporting;

public sealed class MonthlyCodeHealthReportRunner(
    IMonthlyCodeHealthReportUseCase useCase,
    MonthlyCodeHealthReportFormatter formatter)
{
    public async Task<string> Build(DateOnly today, CancellationToken cancellationToken = default)
    {
        var start = new DateOnly(today.Year - 1, 9, 1);
        var rows = await useCase.Build(start, today, cancellationToken).ConfigureAwait(false);
        return formatter.Format(rows);
    }
}
