using System.ComponentModel.DataAnnotations;

namespace WorkCale.Application.DTOs;

public record ShiftDto(
    Guid Id,
    DateOnly Date,
    string StartTime,
    string EndTime,
    string? Location,
    string? Notes,
    int UnpaidBreakMinutes,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    ShiftCategoryDto Category);

public record CreateShiftRequest(
    DateOnly Date,
    [Required] string StartTime,
    [Required] string EndTime,
    [Required] Guid CategoryId,
    string? Location,
    string? Notes,
    int? UnpaidBreakMinutes = 0);

public record UpdateShiftRequest(
    DateOnly Date,
    [Required] string StartTime,
    [Required] string EndTime,
    [Required] Guid CategoryId,
    string? Location,
    string? Notes,
    int? UnpaidBreakMinutes = 0);
