using Microsoft.EntityFrameworkCore;

namespace xproAPI.Models;

public class DataBaseContext : DbContext
{
    public DataBaseContext(DbContextOptions<DataBaseContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<WorkTime> WorkTimes { get; set; }
    public DbSet<BreakDuration> BreakDurations { get; set; }
    public DbSet<Absence> Absences { get; set; }
}