using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Application.DTOs;
using TaskManagementSystem.Domain.Entities;
using TaskManagementSystem.Domain.Enums;
using TaskManagementSystem.Infrastructure.Persistences;
using TaskManagementSystem.Infrastructure.Repositories;

namespace TaskManagementSystem.Infrastructure.Tests.Repositories;

[TestClass]
public sealed class TaskRepositoryTests
{
    private TaskManagementDbContext _db = null!;

    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<TaskManagementDbContext>()
            .UseInMemoryDatabase($"TaskRepoDb_{Guid.NewGuid()}")
            .EnableSensitiveDataLogging()
            .Options;

        _db = new TaskManagementDbContext(options);

        var now = DateTime.UtcNow;
        var tasks = new List<TaskItem>();
        for (int i = 1; i <= 15; i++)
        {
            tasks.Add(new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = $"Task {i} Report",
                Description = (i % 2 == 0) ? "even sample" : "odd sample",
                DueDate = now.AddDays(i),
                IsCompleted = (i % 5 == 0),
                Priority = (i % 3 == 0) ? TaskPriority.High
                         : (i % 3 == 1) ? TaskPriority.Medium
                         : TaskPriority.Low,
                CreatedAtUtc = now.AddDays(-i),
                UpdatedAtUtc = now.AddMinutes(i)
            });
        }
        _db.Tasks.AddRange(tasks);
        _db.SaveChanges();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _db.Database.EnsureDeleted();
        _db.Dispose();
    }

    [TestMethod]
    public async Task Search_Should_Filter_By_Keyword()
    {
        var repo = new TaskRepository(_db);

        var query = new TaskSearchQueryDto(
            Keyword: "Report",
            IsCompleted: null,
            Priority: null,
            DueFromUtc: null,
            DueToUtc: null,
            Page: 1,
            PageSize: 10
        );

        var result = await repo.SearchAsync(query);

        Assert.IsTrue(result.Count > 0);
        Assert.IsTrue(result.All(t =>
            (t.Title?.Contains("Report", StringComparison.OrdinalIgnoreCase) ?? false) ||
            (t.Description ?? string.Empty).Contains("Report", StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public async Task Search_Should_Filter_By_Status_Priority_And_DueRange()
    {
        var repo = new TaskRepository(_db);
        var now = DateTime.UtcNow;

        var query = new TaskSearchQueryDto(
            Keyword: null,
            IsCompleted: false,
            Priority: TaskPriority.Medium,
            DueFromUtc: now.AddDays(1),
            DueToUtc: now.AddDays(10),
            Page: 1,
            PageSize: 50
        );

        var items = await repo.SearchAsync(query);

        Assert.IsTrue(items.Count > 0);
        Assert.IsTrue(items.All(t => !t.IsCompleted));
        Assert.IsTrue(items.All(t => t.Priority == TaskPriority.Medium));
        Assert.IsTrue(items.All(t => t.DueDate >= query.DueFromUtc && t.DueDate <= query.DueToUtc));
    }

    [TestMethod]
    public async Task Search_Should_Order_UpdatedAt_Desc_Then_Priority_Desc_And_Paginate()
    {
        var repo = new TaskRepository(_db);

        var page1 = await repo.SearchAsync(new TaskSearchQueryDto(null, null, null, null, null, 1, 10));
        var page2 = await repo.SearchAsync(new TaskSearchQueryDto(null, null, null, null, null, 2, 10));

        Assert.AreEqual(10, page1.Count);
        Assert.IsTrue(page2.Count <= 10);

        var expectedOrder = await _db.Tasks.AsNoTracking()
            .OrderByDescending(t => t.UpdatedAtUtc)
            .ThenByDescending(t => t.Priority)
            .Select(x => x.Id)
            .ToListAsync();

        CollectionAssert.AreEqual(expectedOrder.Take(10).ToList(), page1.Select(x => x.Id).ToList());
        CollectionAssert.AreEqual(expectedOrder.Skip(10).Take(10).ToList(), page2.Select(x => x.Id).ToList());
    }

    [TestMethod]
    public async Task Search_Should_Return_Empty_When_No_Match()
    {
        var repo = new TaskRepository(_db);

        var items = await repo.SearchAsync(new TaskSearchQueryDto(
            Keyword: "no-match-xyz",
            IsCompleted: null,
            Priority: null,
            DueFromUtc: null,
            DueToUtc: null,
            Page: 1,
            PageSize: 10
        ));

        Assert.AreEqual(0, items.Count);
    }
}
