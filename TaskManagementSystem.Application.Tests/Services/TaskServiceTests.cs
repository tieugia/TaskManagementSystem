using Moq;
using TaskManagementSystem.Application.DTOs;
using TaskManagementSystem.Application.Interfaces.Repositories;
using TaskManagementSystem.Application.Interfaces.Services;
using TaskManagementSystem.Application.Services;
using TaskManagementSystem.Domain.Entities;
using TaskManagementSystem.Domain.Enums;

namespace TaskManagementSystem.Application.Tests.Services;

[TestClass]
public sealed class TaskServiceTests
{
    private Mock<ITaskRepository> _taskRepositoryMock = null!;
    private Mock<ICacheService> _cacheServiceMock = null!;
    private TaskService _taskService = null!;

    [TestInitialize]
    public void Initialize()
    {
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _cacheServiceMock = new Mock<ICacheService>();
        _taskService = new TaskService(_taskRepositoryMock.Object, _cacheServiceMock.Object);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _taskRepositoryMock.VerifyAll();
        _cacheServiceMock.VerifyAll();
    }

    [TestMethod]
    public async Task Create_Should_Trim_Title_Nullify_Empty_Description_And_Clear_SearchCache()
    {
        // Arrange
        var nowBefore = DateTime.UtcNow;
        var input = new TaskCreateDto(
            Title: "  Hello  ",
            Description: "   ",
            DueDate: DateTime.UtcNow.AddDays(2),
            Priority: TaskPriority.High);

        _taskRepositoryMock.Setup(r => r.AddAsync(It.Is<TaskItem>(t =>
                t.Title == "Hello" &&
                t.Description == null &&
                t.DueDate == input.DueDate &&
                t.Priority == TaskPriority.High &&
                !t.IsCompleted &&
                t.CreatedAtUtc >= nowBefore &&
                t.UpdatedAtUtc >= nowBefore)))
            .ReturnsAsync((TaskItem t) =>
            {
                t.Id = Guid.NewGuid();
                t.RowVersion = Guid.NewGuid().ToByteArray();
                return t;
            });

        _cacheServiceMock.Setup(c => c.RemoveByPrefix("tasks:search:"));

        // Act
        var created = await _taskService.CreateTaskAsync(input);

        // Assert
        Assert.AreEqual("Hello", created.Title);
        Assert.IsNull(created.Description);
        Assert.IsFalse(created.IsCompleted);
        Assert.AreEqual(TaskPriority.High, created.Priority);
        Assert.AreNotEqual(Guid.Empty, created.Id);
        Assert.IsNotNull(created.RowVersion);
        _cacheServiceMock.Verify(c => c.RemoveByPrefix("tasks:search:"), Times.Once);
    }

    [TestMethod]
    public async Task GetById_Should_UseCache_If_Available()
    {
        var id = Guid.NewGuid();
        var cached = new TaskDto(
            Id: id,
            Title: "Cached",
            Description: "D",
            DueDate: DateTime.UtcNow,
            IsCompleted: true,
            Priority: TaskPriority.Medium,
            CreatedAtUtc: DateTime.UtcNow,
            UpdatedAtUtc: DateTime.UtcNow,
            RowVersion: [1]);

        _cacheServiceMock.Setup(c => c.TryGet($"tasks:item:{id}", out cached)).Returns(true);

        var result = await _taskService.GetTaskByIdAsync(id);

        Assert.AreEqual("Cached", result.Title);
        _taskRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [TestMethod]
    public async Task GetById_Should_Fetch_FromRepo_And_Cache()
    {
        var id = Guid.NewGuid();
        var entity = new TaskItem
        {
            Id = id,
            Title = "T",
            Description = "D",
            DueDate = DateTime.UtcNow,
            IsCompleted = true,
            Priority = TaskPriority.Medium,
            CreatedAtUtc = DateTime.UtcNow.AddDays(-1),
            UpdatedAtUtc = DateTime.UtcNow,
            RowVersion = [1, 2]
        };

        _cacheServiceMock.Setup(c => c.TryGet($"tasks:item:{id}", out It.Ref<TaskDto?>.IsAny))
                         .Returns(false);
        _taskRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);
        _cacheServiceMock.Setup(c => c.Set($"tasks:item:{id}", It.IsAny<TaskDto>(), It.IsAny<TimeSpan>()));

        var dto = await _taskService.GetTaskByIdAsync(id);

        Assert.AreEqual(id, dto.Id);
        _cacheServiceMock.Verify(c => c.Set($"tasks:item:{id}", It.IsAny<TaskDto>(), It.IsAny<TimeSpan>()), Times.Once);
    }

    [TestMethod]
    public async Task GetById_Should_Throw_When_NotFound()
    {
        var id = Guid.NewGuid();
        _cacheServiceMock.Setup(c => c.TryGet($"tasks:item:{id}", out It.Ref<TaskDto?>.IsAny))
                         .Returns(false);
        _taskRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((TaskItem?)null);

        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(() => _taskService.GetTaskByIdAsync(id));
    }

    [TestMethod]
    public async Task Update_Should_Load_Apply_Changes_And_Clear_Cache()
    {
        var id = Guid.NewGuid();
        var existing = new TaskItem
        {
            Id = id,
            Title = "Old",
            Description = "desc",
            DueDate = DateTime.UtcNow,
            Priority = TaskPriority.Low,
            IsCompleted = false,
            CreatedAtUtc = DateTime.UtcNow.AddDays(-3),
            UpdatedAtUtc = DateTime.UtcNow.AddDays(-1),
            RowVersion = [9, 9]
        };

        _taskRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);
        _taskRepositoryMock.Setup(r => r.UpdateAsync(It.Is<TaskItem>(t => t.Title == "New" && t.Description == null)));
        _cacheServiceMock.Setup(c => c.Remove($"tasks:item:{id}"));
        _cacheServiceMock.Setup(c => c.RemoveByPrefix("tasks:search:"));

        var upd = new TaskUpdateDto(
            Id: id,
            Title: " New ",
            Description: "  ",
            DueDate: DateTime.UtcNow.AddDays(5),
            Priority: TaskPriority.High,
            IsCompleted: true,
            RowVersion: existing.RowVersion
        );

        await _taskService.UpdateTaskAsync(upd);

        _cacheServiceMock.Verify(c => c.Remove($"tasks:item:{id}"), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveByPrefix("tasks:search:"), Times.Once);
    }

    [TestMethod]
    public async Task Update_Should_Throw_When_NotFound()
    {
        _taskRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((TaskItem?)null);

        var upd = new TaskUpdateDto(Guid.NewGuid(), "T", null, null, TaskPriority.Low, false, null!);
        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(() => _taskService.UpdateTaskAsync(upd));
    }

    [TestMethod]
    public async Task Delete_Should_Load_And_Clear_Cache()
    {
        var id = Guid.NewGuid();
        var existing = new TaskItem { Id = id };

        _taskRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);
        _taskRepositoryMock.Setup(r => r.DeleteAsync(existing));
        _cacheServiceMock.Setup(c => c.Remove($"tasks:item:{id}"));
        _cacheServiceMock.Setup(c => c.RemoveByPrefix("tasks:search:"));

        await _taskService.DeleteTaskAsync(id);

        _cacheServiceMock.Verify(c => c.Remove($"tasks:item:{id}"), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveByPrefix("tasks:search:"), Times.Once);
    }

    [TestMethod]
    public async Task Delete_Should_Throw_When_NotFound()
    {
        _taskRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((TaskItem?)null);

        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(() => _taskService.DeleteTaskAsync(Guid.NewGuid()));
    }

    [TestMethod]
    public async Task Search_Should_Use_Cache_If_Available()
    {
        // Arrange
        var query = new TaskSearchQueryDto("k", null, null, null, null, 1, 10);
        var cacheKey = $"tasks:search:{query.Keyword}:{query.IsCompleted}:{query.Priority}:{query.Page}:{query.PageSize}:{query.DueFromUtc}:{query.DueToUtc}";
        var cachedList = new List<TaskDto> { new(Guid.NewGuid(), "Cached", null, null, false, TaskPriority.Low, DateTime.UtcNow, DateTime.UtcNow, null!) };

        IEnumerable<TaskDto>? outValue = cachedList;

        _cacheServiceMock.Setup(c => c.TryGet(cacheKey, out outValue)).Returns(true);
        
        // Act
        var result = (await _taskService.SearchAsync(query)).ToList();

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Cached", result[0].Title);
        _taskRepositoryMock.Verify(r => r.SearchAsync(It.IsAny<TaskSearchQueryDto>()), Times.Never);
    }

    [TestMethod]
    public async Task Search_Should_Call_Repo_And_Cache()
    {
        var query = new TaskSearchQueryDto("k", null, null, null, null, 1, 10);
        var cacheKey = $"tasks:search:{query.Keyword}:{query.IsCompleted}:{query.Priority}:{query.Page}:{query.PageSize}:{query.DueFromUtc}:{query.DueToUtc}";
        _cacheServiceMock.Setup(c => c.TryGet(cacheKey, out It.Ref<IEnumerable<TaskDto>?>.IsAny))
                         .Returns(false);

        var entities = new List<TaskItem>
        {
            new() { Id = Guid.NewGuid(), Title = "A", Priority = TaskPriority.Low, CreatedAtUtc = DateTime.UtcNow, UpdatedAtUtc = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Title = "B", Priority = TaskPriority.High, CreatedAtUtc = DateTime.UtcNow, UpdatedAtUtc = DateTime.UtcNow }
        };

        _taskRepositoryMock.Setup(r => r.SearchAsync(query)).ReturnsAsync(entities);
        _cacheServiceMock.Setup(c => c.Set(cacheKey, It.IsAny<IEnumerable<TaskDto>>(), It.IsAny<TimeSpan>()));

        var result = (await _taskService.SearchAsync(query)).ToList();

        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.Any(x => x.Title == "A"));
        Assert.IsTrue(result.Any(x => x.Title == "B"));
        _cacheServiceMock.Verify(c => c.Set(cacheKey, It.IsAny<IEnumerable<TaskDto>>(), It.IsAny<TimeSpan>()), Times.Once);
    }
}
