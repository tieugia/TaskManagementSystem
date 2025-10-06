using TaskManagementSystem.API.Models;
using TaskManagementSystem.Application.DTOs;

namespace TaskManagementSystem.API.Mappers;

public static class RequestMappers
{
    public static TaskSearchQueryDto ToDto(this TaskSearchQueryRequest vm) => new
    (
        vm.Keyword,
        vm.IsCompleted,
        vm.Priority,
        vm.DueFromUtc,
        vm.DueToUtc,
        vm.Page,
        vm.PageSize
    );

    public static TaskCreateDto ToDto(this TaskCreateRequest vm)
        => new(vm.Title, vm.Description, vm.DueDate, vm.Priority);

    public static TaskUpdateDto ToDto(this TaskUpdateRequest vm)
        => new(vm.Id, vm.Title, vm.Description, vm.DueDate, vm.Priority, vm.IsCompleted, vm.RowVersion);
}
