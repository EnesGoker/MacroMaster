namespace MacroMaster.Domain.Models;

public sealed class MacroSession
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = "New Macro";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public string FormatVersion { get; set; } = "1.0";

    public List<MacroEvent> Events { get; set; } = new();

    public int TotalEventCount => Events.Count;

    public int TotalDurationMs => Events.Sum(e => e.DelayMs);
}