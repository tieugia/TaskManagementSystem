using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using TaskManagementSystem.API.Models;
using TaskManagementSystem.Application.DTOs;
using TaskManagementSystem.Domain.Enums;

namespace TaskManagementSystem.API.Tests.Controllers
{
    [TestClass]
    public class TasksControllerTests
    {
        private const string BaseUrl = "/api/tasks";

        private WebApplicationFactory<Program> _factory = null!;
        private HttpClient _client = null!;

        [TestInitialize]
        public void Init()
        {
            _factory = new WebApplicationFactory<Program>();
            _client = _factory.CreateClient();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _factory.Dispose();
            _client.Dispose();
        }

        [TestMethod]
        public async Task Post_ValidTask_ReturnsCreated()
        {
            // Arrange
            var newTask = new TaskCreateRequest
            {
                Title = "Integration Test Task",
                Description = "Created by MSTest integration test",
                DueDate = DateTime.UtcNow.AddDays(3),
                Priority = TaskPriority.Medium
            };

            // Act
            var response = await _client.PostAsJsonAsync(BaseUrl, newTask);

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var created = await response.Content.ReadFromJsonAsync<TaskDto>();
            Assert.IsNotNull(created);
            Assert.AreNotEqual(Guid.Empty, created!.Id);
            Assert.AreEqual(newTask.Title, created.Title);
        }

        [TestMethod]
        public async Task Get_InvalidId_ReturnsNotFound()
        {
            // Arrange
            var fakeId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"{BaseUrl}/{fakeId}");

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            if (body != null && body.TryGetValue("message", out var msg))
            {
                StringAssert.Contains(msg.ToLowerInvariant(), "not found");
            }
        }

        [TestMethod]
        public async Task Delete_InvalidId_ReturnsNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            var response = await _client.DeleteAsync($"{BaseUrl}/{id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Put_WithMismatchedId_ReturnsBadRequest()
        {
            // Arrange
            var id = Guid.NewGuid();

            var payload = new TaskUpdateRequest
            {
                Id = Guid.NewGuid(), // mismatch
                Title = "Mismatch Title",
                Description = "Mismatch Description",
                DueDate = DateTime.UtcNow.AddDays(5),
                Priority = TaskPriority.High,
                IsCompleted = false,
                RowVersion = []
            };

            // Act
            var response = await _client.PutAsJsonAsync($"{BaseUrl}/{id}", payload);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task Get_WithValidQuery_ReturnsOk()
        {
            // Arrange
            var query = new TaskSearchQueryRequest
            {
                Keyword = "test",
                Page = 1,
                PageSize = 5
            };

            var queryString = $"?keyword={query.Keyword}&page={query.Page}&pageSize={query.PageSize}";

            // Act
            var response = await _client.GetAsync($"{BaseUrl}{queryString}");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
