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
    
    public static void Seed(DataBaseContext context)
    {
        // Check if data already exists
        if (!context.Users.Any())
        {
            context.Users.AddRange(new User
                {
                    Name = "John",
                    Surname = "Doe",
                    Phone = "(386) 12/345-678",
                    Email = "john@example.com",
                    Position = "Developer",
                    EmploymentType = "Full time",
                    Role = "user",
                    Active = true,
                    Username = "johndoe",
                    Password = BCrypt.Net.BCrypt.EnhancedHashPassword("Password123!")
                },
                new User
                {
                    Name = "Admin",
                    Surname = "XPRO",
                    Phone = "(386) 12/345-678",
                    Email = "xproadmin@example.com",
                    EmploymentType = "Full time",
                    Position = "Senior Developer",
                    Role = "admin",
                    Active = true,
                    Username = "admin",
                    Password = BCrypt.Net.BCrypt.EnhancedHashPassword("Password123!")
                });
            context.SaveChanges();
            context.BreakDurations.Add(
                new BreakDuration()
                {
                    Duration = 30,
                    Valid = true
                });
            context.SaveChanges();
        }
    }
}