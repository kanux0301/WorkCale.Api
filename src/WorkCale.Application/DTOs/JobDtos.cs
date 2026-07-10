using System.ComponentModel.DataAnnotations;

namespace WorkCale.Application.DTOs;

public record JobDto(
    Guid Id,
    string Name,
    string Color,
    string? Icon,
    bool IsDefault,
    bool IsArchived,
    int SortOrder,
    DateTime CreatedAt);

public record CreateJobRequest(
    [Required, MaxLength(60)] string Name,
    [Required] string Color,
    [MaxLength(50)] string? Icon = null);

public record UpdateJobRequest(
    [Required, MaxLength(60)] string Name,
    [Required] string Color,
    [MaxLength(50)] string? Icon = null);
