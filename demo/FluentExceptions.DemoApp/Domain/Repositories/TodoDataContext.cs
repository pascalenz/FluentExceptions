using FluentExceptions.DemoApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FluentExceptions.DemoApp.Domain.Repositories;

public class TodoDataContext(DbContextOptions<TodoDataContext> options) : DbContext(options)
{
    public DbSet<Todo> Todos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Todo>().ToTable("Todo").HasKey(e => e.Id);
        modelBuilder.Entity<Todo>().HasIndex(e => e.Id).IsUnique();
        modelBuilder.Entity<Todo>().HasIndex(e => e.Title).IsUnique();
        modelBuilder.Entity<Todo>().Property(e => e.Id).ValueGeneratedOnAdd();
    }
}
