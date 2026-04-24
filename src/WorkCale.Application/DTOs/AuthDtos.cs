using System.ComponentModel.DataAnnotations;

namespace WorkCale.Application.DTOs;

public record UserDto(
    Guid Id,
    string Email,
    string DisplayName,
    string? AvatarUrl,
    string? AvatarColor = null,
    string? AvatarIcon = null);

public record AuthResult(
    string AccessToken,
    string RefreshToken,
    UserDto User);

public record LoginRequest(
    [Required] string Email,
    [Required] string Password);

public record RegisterRequest(
    [Required, MaxLength(255)] string Email,
    [Required, MaxLength(100)] string DisplayName,
    [Required] string Password);

public record GoogleLoginRequest(
    [Required] string IdToken);

public record RefreshRequest(
    [Required] string RefreshToken);

public record LogoutRequest(
    [Required] string RefreshToken);

public record UpdateProfileRequest(
    [Required, MaxLength(100)] string DisplayName,
    [MaxLength(7)] string? AvatarColor = null,
    [MaxLength(60)] string? AvatarIcon = null);

public record ChangePasswordRequest(
    [Required] string CurrentPassword,
    [Required] string NewPassword);
