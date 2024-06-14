using Microsoft.EntityFrameworkCore;

namespace FluentExceptions.EntityFrameworkCore.Tests;

internal class TestDataContext(DbContextOptions<TestDataContext> options) : DbContext(options)
{
    public DbSet<TestEntity> Courses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestEntity>().ToTable("test").HasKey(e => e.Id);
    }
}
