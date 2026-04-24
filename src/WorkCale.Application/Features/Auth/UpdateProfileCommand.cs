using MediatR;
using WorkCale.Application.DTOs;

namespace WorkCale.Application.Features.Auth;

public record UpdateProfileCommand(
    Guid UserId,
    string DisplayName,
    string? AvatarColor = null,
    string? AvatarIcon = null) : IRequest<UserDto>;
