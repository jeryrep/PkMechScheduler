using Microsoft.EntityFrameworkCore;
using PkMechScheduler.Database.Models;

namespace PkMechScheduler.Database;

public sealed class SchedulerContext : DbContext
{
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<StudentBlock> StudentBlocks => Set<StudentBlock>();
    public DbSet<TeacherBlock> TeacherBlocks => Set<TeacherBlock>();
    public string DbPath { get; }

    public SchedulerContext()
    {
        const Environment.SpecialFolder folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = Path.Join(path, "scheduler.db");
    }
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