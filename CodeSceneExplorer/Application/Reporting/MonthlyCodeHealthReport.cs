namespace CodeSceneExplorer.Application.Reporting;

public sealed record MonthlyCodeHealthReport(
    IReadOnlyList<MonthlyCodeHealthRow> MonthlyRows,
    IReadOnlyList<MonthlyCodeHealthThresholdCounts> ThresholdCounts,
    IReadOnlyList<ProjectCodeHealthTrend> LargestRegressions,
    MonthlyCodeHealthRecentTrendSummary? RecentTrendSummary);

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

public sealed record MonthlyCodeHealthThresholdCounts(
    string YearMonth,
    int TotalProjects,
    int Below5,
    int Below7,
    int Below8);

public sealed record MonthlyCodeHealthRecentTrendSummary(
    string WindowStartYearMonth,
    string WindowEndYearMonth,
    int DecliningProjects,
    int ImprovingProjects,
    int StableProjects,
    IReadOnlyList<ProjectCodeHealthTrend> DecliningProjectDetails)
{
    public string Window => $"{WindowStartYearMonth} to {WindowEndYearMonth}";
}
