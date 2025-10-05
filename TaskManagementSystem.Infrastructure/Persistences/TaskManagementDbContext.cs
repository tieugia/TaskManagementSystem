using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Common.Extensions;
using TaskManagementSystem.Domain.Entities;

namespace TaskManagementSystem.Infrastructure.Persistences;

public sealed class TaskManagementDbContext(DbContextOptions<TaskManagementDbContext> options) : DbContext(options)
{
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entityBuilder = modelBuilder.Entity<TaskItem>();
        var rowVersion = entityBuilder
            .Property(c => c.RowVersion)
            .IsRowVersion();
        
        if (Database.IsInMemory())
            rowVersion.HasValueGenerator<InMemoryRowVersionGenerator>();

        entityBuilder.HasKey(x => x.Id);
        entityBuilder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        entityBuilder.Property(x => x.Description).HasMaxLength(2000);
        entityBuilder.Property(x => x.CreatedAtUtc).HasPrecision(0);
        entityBuilder.Property(x => x.UpdatedAtUtc).HasPrecision(0);
        entityBuilder.HasIndex(x => x.IsCompleted);
        entityBuilder.HasIndex(x => x.Priority);
        entityBuilder.HasIndex(x => x.DueDate);

        base.OnModelCreating(modelBuilder);
    }
}