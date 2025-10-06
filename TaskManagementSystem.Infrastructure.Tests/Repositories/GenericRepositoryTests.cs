using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Domain.Entities;
using TaskManagementSystem.Domain.Enums;
using TaskManagementSystem.Infrastructure.Persistences;
using TaskManagementSystem.Infrastructure.Repositories;

namespace TaskManagementSystem.Infrastructure.Tests.Repositories;

[TestClass]
public sealed class GenericRepositoryTests
{
    private TaskManagementDbContext _db = null!;

    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<TaskManagementDbContext>()
            .UseInMemoryDatabase($"GenericRepoDb_{Guid.NewGuid()}")
            .EnableSensitiveDataLogging()
            .Options;

        _db = new TaskManagementDbContext(options);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _db.Database.EnsureDeleted();
        _db.Dispose();
    }

    [TestMethod]
    public async Task Add_GetAll_GetById_Should_Work()
    {
        var repo = new GenericRepository<TaskItem>(_db);

        var e = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "New",
            Description = null,
            DueDate = null,
            IsCompleted = false,
            Priority = TaskPriority.Low,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        var added = await repo.AddAsync(e);
        Assert.AreEqual(e.Id, added.Id);
        Assert.AreEqual("New", added.Title);

        var all = await repo.GetAllAsync();
        Assert.IsTrue(all.Any(x => x.Id == e.Id));

        var byId = await repo.GetByIdAsync(e.Id);
        Assert.IsNotNull(byId);
        Assert.AreEqual(e.Id, byId.Id);
    }

    [TestMethod]
    public async Task Update_Should_Persist_Changes_And_Respect_RowVersion_OriginalValue()
    {
        var repo = new GenericRepository<TaskItem>(_db);

        // Seed entity
        var seeded = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Before",
            Description = null,
            DueDate = null,
            IsCompleted = false,
            Priority = TaskPriority.Medium,
            CreatedAtUtc = DateTime.UtcNow.AddDays(-1),
            UpdatedAtUtc = DateTime.UtcNow.AddDays(-1),
            RowVersion = [1, 2, 3]
        };
        _db.Tasks.Add(seeded);
        await _db.SaveChangesAsync();
        
        _db.ChangeTracker.Clear();
        
        var detached = new TaskItem
        {
            Id = seeded.Id,
            Title = "After",
            Description = "updated",
            DueDate = DateTime.UtcNow,
            IsCompleted = true,
            Priority = TaskPriority.High,
            CreatedAtUtc = seeded.CreatedAtUtc,
            UpdatedAtUtc = DateTime.UtcNow,
            RowVersion = seeded.RowVersion
        };

        await repo.UpdateAsync(detached);

        var updated = await _db.Tasks.AsNoTracking().FirstAsync(x => x.Id == seeded.Id);
        Assert.AreEqual("After", updated.Title);
        Assert.AreEqual("updated", updated.Description);
        Assert.IsTrue(updated.IsCompleted);
        Assert.AreEqual(TaskPriority.High, updated.Priority);
    }

    [TestMethod]
    public async Task Delete_Should_Remove_Entity()
    {
        var repo = new GenericRepository<TaskItem>(_db);

        var e = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "ToDelete",
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            Priority = TaskPriority.Low
        };

        _db.Tasks.Add(e);
        await _db.SaveChangesAsync();

        await repo.DeleteAsync(e);

        var exists = await _db.Tasks.AsNoTracking().AnyAsync(x => x.Id == e.Id);
        Assert.IsFalse(exists);
    }
}
