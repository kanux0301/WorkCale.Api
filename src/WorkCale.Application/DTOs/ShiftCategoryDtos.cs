using System.ComponentModel.DataAnnotations;

namespace WorkCale.Application.DTOs;

public record ShiftCategoryDto(
    Guid Id,
    string Name,
    string Color,
    string? DefaultStartTime,
    string? DefaultEndTime,
    DateTime CreatedAt);

public record CreateCategoryRequest(
    [Required, MaxLength(50)] string Name,
    [Required] string Color,
    string? DefaultStartTime,
    string? DefaultEndTime);

public record UpdateCategoryRequest(
    [Required, MaxLength(50)] string Name,
    [Required] string Color,
    string? DefaultStartTime,
    string? DefaultEndTime);
