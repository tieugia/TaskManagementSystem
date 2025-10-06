using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.API.Mappers;
using TaskManagementSystem.API.Models;
using TaskManagementSystem.Application.DTOs;
using TaskManagementSystem.Application.Interfaces.Services;
using TaskManagementSystem.Common.Attributes;

namespace TaskManagementSystem.API.Controllers;

[Route("api/[controller]")]
[ApiController]

public class TasksController(ITaskService taskService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTasks(
        [FromQuery] TaskSearchQueryRequest queryViewModel)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var tasks = await taskService.SearchAsync(queryViewModel.ToDto());
        return Ok(tasks);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskDto>> GetTask(Guid id)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        var task = await taskService.GetTaskByIdAsync(id);
        return Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<TaskDto>> CreateAsync([FromBody] TaskCreateRequest taskCreateViewModel)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        var createdTask = await taskService.CreateTaskAsync(taskCreateViewModel.ToDto());
        return CreatedAtAction(nameof(GetTask), new { id = createdTask.Id }, createdTask);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask([NotEmptyGuid] Guid id, TaskUpdateRequest taskUpdateViewModel)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (id != taskUpdateViewModel.Id)
            return BadRequest("Task ID mismatch");

        await taskService.UpdateTaskAsync(taskUpdateViewModel.ToDto());
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask([NotEmptyGuid] Guid id)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await taskService.DeleteTaskAsync(id);
        return NoContent();
    }
}

