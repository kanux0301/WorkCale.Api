using System.ComponentModel.DataAnnotations;

namespace WorkCale.Application.DTOs;

public record ShiftCategoryDto(
    Guid Id,
    string Name,
    string Color,
    DateTime CreatedAt);

public record CreateCategoryRequest(
    [Required, MaxLength(50)] string Name,
    [Required] string Color);

public record UpdateCategoryRequest(
    [Required, MaxLength(50)] string Name,
    [Required] string Color);
