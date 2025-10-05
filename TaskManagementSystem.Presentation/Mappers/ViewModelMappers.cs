using TaskManagementSystem.Application.DTOs;
using TaskManagementSystem.Presentation.Models;

namespace TaskManagementSystem.Presentation.Mappers;

public static class ViewModelMappers
{
    public static TaskSearchQueryDto ToDto(this TaskSearchQueryViewModel vm) => new
    (
        vm.Keyword,
        vm.IsCompleted,
        vm.Priority,
        vm.DueFromUtc,
        vm.DueToUtc,
        vm.Page,
        vm.PageSize,
        vm.RowVersion
    );

    public static TaskCreateDto ToDto(this TaskCreateViewModel vm)
        => new(vm.Title, vm.Description, vm.DueDate, vm.Priority);

    public static TaskUpdateDto ToDto(this TaskUpdateViewModel vm)
        => new(vm.Id, vm.Title, vm.Description, vm.DueDate, vm.Priority, vm.IsCompleted, vm.RowVersion);
}
