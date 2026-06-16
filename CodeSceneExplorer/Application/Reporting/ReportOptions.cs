using System.Globalization;

namespace CodeSceneExplorer.Application.Reporting;

public sealed class ReportOptions
{
    public string? StartDate { get; init; }

    public DateOnly? GetStartDate() =>
        StartDate is not null
            ? DateOnly.Parse(StartDate, CultureInfo.InvariantCulture)
            : null;
}
