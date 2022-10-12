using Microsoft.EntityFrameworkCore;
using PkMechScheduler.Database.Models;

namespace PkMechScheduler.Database;

public sealed class SchedulerContext : DbContext
{
    public DbSet<Group> Groups { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<StudentBlock> StudentBlocks { get; set; }
    public DbSet<TeacherBlock> TeacherBlocks { get; set; }
    public string DbPath { get; }

    public SchedulerContext(){}
    public SchedulerContext(DbContextOptions<SchedulerContext> options) : base(options)
    {
        const Environment.SpecialFolder folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = Path.Join(path, "scheduler.db");
        //uncomment following line to restart db for example after migration
        //File.Delete(DbPath);
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite($"Data Source={DbPath}");
}