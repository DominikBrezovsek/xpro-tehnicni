namespace xproAPI.Models;

public class WorkTime
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long BreakDurationId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly? ClockIn { get; set; }
    public TimeOnly? ClockOut { get; set; }
    public String? TimeZone { get; set; }
    public string? TotalWorkTime { get; set; }
    public TimeOnly? BreakStart { get; set; }
    public TimeOnly? BreakEnd { get; set; }
    public TimeOnly? BreakDuration { get; set; }
    public TimeOnly? BreakOverAllowedTime { get; set; }
    public bool Absent { get; set; } = false;
    public long? AbsentType { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}