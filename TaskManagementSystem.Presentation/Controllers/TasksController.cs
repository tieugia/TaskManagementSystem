using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.Application.DTOs;
using TaskManagementSystem.Application.Interfaces.Services;
using TaskManagementSystem.Common.Attributes;
using TaskManagementSystem.Presentation.Mappers;
using TaskManagementSystem.Presentation.Models;

namespace TaskManagementSystem.Presentation.Controllers;

public sealed class TasksController(ITaskService taskService) : Controller
{
    public async Task<IActionResult> Index([FromQuery] TaskSearchQueryViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Query = vm.Keyword;
            ViewBag.IsDone = vm.IsCompleted;
            ViewBag.Priority = vm.Priority;
            ViewBag.Page = vm.Page;
            ViewBag.Size = vm.PageSize;
            
            return View(Array.Empty<TaskDto>());
        }

        var result = await taskService.SearchAsync(vm.ToDto());
        
        ViewBag.Query = vm.Keyword;
        ViewBag.IsDone = vm.IsCompleted;
        ViewBag.Priority = vm.Priority;
        ViewBag.Page = vm.Page; 
        ViewBag.Size = vm.PageSize;
        
        return View(result);
    }
    
    public async Task<IActionResult> Create()
    {        
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TaskCreateViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        
        await taskService.CreateTaskAsync(vm.ToDto());
        
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit([NotEmptyGuid] Guid id)
    {
        var taskDto = await taskService.GetTaskByIdAsync(id);

        var vm = new TaskUpdateViewModel
        {
            Id = taskDto.Id,
            Title = taskDto.Title,
            Description = taskDto.Description,
            DueDate = taskDto.DueDate,
            Priority = taskDto.Priority,
            IsCompleted = taskDto.IsCompleted,
            RowVersion = taskDto.RowVersion
        };
        
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(TaskUpdateViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        
        await taskService.UpdateTaskAsync(vm.ToDto());
        
        return RedirectToAction(nameof(Details), new { vm.Id });
    }

    public async Task<IActionResult> Details([NotEmptyGuid] Guid id)
        => await taskService.GetTaskByIdAsync(id) is { } t ? View(t) : NotFound();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        await taskService.DeleteTaskAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
