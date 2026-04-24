using MediatR;
using WorkCale.Application.DTOs;
using WorkCale.Application.Services;

namespace WorkCale.Application.Features.Auth;

public class UpdateProfileCommandHandler(IUserRepository userRepository)
    : IRequestHandler<UpdateProfileCommand, UserDto>
{
    public async Task<UserDto> Handle(UpdateProfileCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, ct)
                   ?? throw new KeyNotFoundException("User not found.");

        var trimmedName = request.DisplayName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmedName))
            throw new ArgumentException("Display name cannot be empty.", nameof(request.DisplayName));

        // Light validation: color must look like a 7-char hex, icon reasonably short.
        // The mobile picker already constrains the value, so we just defend the boundary.
        if (request.AvatarColor is { Length: > 0 } col
            && !(col.StartsWith('#') && col.Length == 7))
            throw new ArgumentException("AvatarColor must be a 7-character hex string like #4C6FA3.");

        user.UpdateProfile(trimmedName, request.AvatarColor, request.AvatarIcon);
        await userRepository.UpdateAsync(user, ct);

        return new UserDto(user.Id, user.Email, user.DisplayName, user.AvatarUrl, user.AvatarColor, user.AvatarIcon);
    }
}
