namespace CodeSceneExplorer.Application.Reporting;

public sealed record MonthlyCodeHealthReport(
    IReadOnlyList<MonthlyCodeHealthRow> MonthlyRows,
    IReadOnlyList<ProjectCodeHealthTrend> TopDecliners,
    IReadOnlyList<ProjectCodeHealthTrend> SmallImprovers);

public sealed record ProjectCodeHealthTrend(
    int ProjectId,
    string? ProjectName,
    IReadOnlyList<MonthlyCodeHealthReading> Readings,
    decimal StartCodeHealth,
    decimal EndCodeHealth)
{
    public decimal Delta => EndCodeHealth - StartCodeHealth;

    public string DisplayName =>
        string.IsNullOrWhiteSpace(ProjectName)
            ? $"Project {ProjectId}"
            : $"{ProjectName} ({ProjectId})";
}
