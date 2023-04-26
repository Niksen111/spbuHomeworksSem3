namespace MyNUnitWeb.Data;

using Microsoft.EntityFrameworkCore;

public class TestingDataDbContext : DbContext
{
    public TestingDataDbContext(DbContextOptions<TestingDataDbContext> options)
        : base(options)
    {
    }

    public DbSet<Assembly> Assemblies { get; set; }

    public DbSet<Test> Tests { get; set; }
}