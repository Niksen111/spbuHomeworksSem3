namespace MyNUnitWeb.Data;

using Microsoft.EntityFrameworkCore;

public class TestingDataDbContext : DbContext
{
    public TestingDataDbContext(DbContextOptions<TestingDataDbContext> options)
        : base(options)
    {
    }

    public DbSet<Assembly> Assemblies => Set<Assembly>();

    public DbSet<Test> Tests => Set<Test>();
}