using Moq;
using TaskManagementSystem.Application.DTOs;
using TaskManagementSystem.Application.Interfaces.Repositories;
using TaskManagementSystem.Application.Services;
using TaskManagementSystem.Domain.Entities;
using TaskManagementSystem.Domain.Enums;

namespace TaskManagementSystem.Application.Tests.Services;

[TestClass]
public sealed class TaskServiceTests
{
    private Mock<ITaskRepository> _taskRepositoryMock = null!;
    private TaskService _taskService = null!;

    [TestInitialize]
    public void Initialize()
    {
        _taskRepositoryMock = new Mock<ITaskRepository>(MockBehavior.Strict);
        _taskService = new TaskService(_taskRepositoryMock.Object);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _taskRepositoryMock.VerifyAll();
    }

    [TestMethod]
    public async Task Create_Should_Trim_Title_Nullify_Empty_Description_And_Map_Out()
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
                t.IsCompleted == false &&
                t.CreatedAtUtc >= nowBefore &&
                t.UpdatedAtUtc >= nowBefore)))
            .ReturnsAsync((TaskItem t) =>
            {
                t.Id = Guid.NewGuid();
                t.RowVersion = Guid.NewGuid().ToByteArray();
                return t;
            });

        // Act
        var created = await _taskService.CreateTaskAsync(input);

        // Assert
        Assert.AreEqual("Hello", created.Title);
        Assert.IsNull(created.Description);
        Assert.IsFalse(created.IsCompleted);
        Assert.AreEqual(TaskPriority.High, created.Priority);
        Assert.AreNotEqual(Guid.Empty, created.Id);
        Assert.IsNotNull(created.RowVersion);
        Assert.IsTrue(created.CreatedAtUtc >= nowBefore);
        Assert.IsTrue(created.UpdatedAtUtc >= nowBefore);
    }

    [TestMethod]
    public async Task GetById_Should_Return_Dto()
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
            RowVersion = new byte[] { 1, 2 }
        };
        _taskRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);

        var dto = await _taskService.GetTaskByIdAsync(id);

        Assert.AreEqual(id, dto.Id);
        Assert.AreEqual("T", dto.Title);
        Assert.AreEqual("D", dto.Description);
        Assert.IsTrue(dto.IsCompleted);
        Assert.AreEqual(TaskPriority.Medium, dto.Priority);
        Assert.IsNotNull(dto.RowVersion);
    }

    [TestMethod]
    public async Task GetById_Should_Throw_When_NotFound()
    {
        _taskRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((TaskItem?)null);

        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(() => _taskService.GetTaskByIdAsync(Guid.NewGuid()));
    }

    [TestMethod]
    public async Task Update_Should_Load_Apply_Changes_And_Call_Update()
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
            RowVersion = new byte[] { 9, 9 }
        };

        _taskRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);
        _taskRepositoryMock.Setup(r => r.UpdateAsync(It.Is<TaskItem>(t =>
                t.Id == id &&
                t.Title == "New" &&
                t.Description == null &&
                t.Priority == TaskPriority.High &&
                t.IsCompleted == true &&
                t.DueDate.HasValue &&
                t.RowVersion == existing.RowVersion
        ))).Returns(Task.CompletedTask);

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
    }

    [TestMethod]
    public async Task Update_Should_Throw_When_NotFound()
    {
        _taskRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((TaskItem?)null);

        var upd = new TaskUpdateDto(Guid.NewGuid(), "T", null, null, TaskPriority.Low, false, null);
        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(() => _taskService.UpdateTaskAsync(upd));
    }

    [TestMethod]
    public async Task Delete_Should_Load_And_Call_Delete()
    {
        var id = Guid.NewGuid();
        var existing = new TaskItem { Id = id };

        _taskRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);
        _taskRepositoryMock.Setup(r => r.DeleteAsync(existing)).Returns(Task.CompletedTask);

        await _taskService.DeleteTaskAsync(id);
    }

    [TestMethod]
    public async Task Delete_Should_Throw_When_NotFound()
    {
        _taskRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((TaskItem?)null);

        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(() => _taskService.DeleteTaskAsync(Guid.NewGuid()));
    }

    [TestMethod]
    public async Task Search_Should_Call_Repository_And_Map()
    {
        TaskItem[] entities =
        [
            new() { Id = Guid.NewGuid(), Title = "A", CreatedAtUtc = DateTime.UtcNow, UpdatedAtUtc = DateTime.UtcNow, Priority = TaskPriority.Low },
            new() { Id = Guid.NewGuid(), Title = "B", CreatedAtUtc = DateTime.UtcNow, UpdatedAtUtc = DateTime.UtcNow, Priority = TaskPriority.High }
        ];

        var query = new TaskSearchQueryDto(
            Keyword: "k", IsCompleted: null, Priority: null,
            DueFromUtc: null, DueToUtc: null, Page: 1, PageSize: 10, []);

        _taskRepositoryMock.Setup(r => r.SearchAsync(query)).ReturnsAsync(entities.ToList());

        var result = (await _taskService.SearchAsync(query)).ToList();

        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.Any(x => x.Title == "A"));
        Assert.IsTrue(result.Any(x => x.Title == "B"));
    }
}
