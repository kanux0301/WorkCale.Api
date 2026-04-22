using System.ComponentModel.DataAnnotations;

namespace WorkCale.Application.DTOs;

public record ShiftCategoryDto(
    Guid Id,
    string Name,
    string Color,
    string? DefaultStartTime,
    string? DefaultEndTime,
    string? Icon,
    DateTime CreatedAt);

public record CreateCategoryRequest(
    [Required, MaxLength(50)] string Name,
    [Required] string Color,
    string? DefaultStartTime,
    string? DefaultEndTime,
    [MaxLength(50)] string? Icon = null);

public record UpdateCategoryRequest(
    [Required, MaxLength(50)] string Name,
    [Required] string Color,
    string? DefaultStartTime,
    string? DefaultEndTime,
    [MaxLength(50)] string? Icon = null);
