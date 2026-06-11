namespace CodeSceneExplorer.Domain.Shared;

public sealed record DateRange(DateOnly From, DateOnly To)
{
    public static DateRange Create(DateOnly from, DateOnly to)
    {
        if (from > to)
        {
            throw new ArgumentException("The start date must be on or before the end date.", nameof(from));
        }

        return new DateRange(from, to);
    }
}
