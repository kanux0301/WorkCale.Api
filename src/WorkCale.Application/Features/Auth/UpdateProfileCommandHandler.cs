using MediatR;
using WorkCale.Application.DTOs;
using WorkCale.Application.Mappings;
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

        if (request.AvatarColor is { Length: > 0 } col && (col.Length != 7 || !col.StartsWith('#')))
            throw new ArgumentException("AvatarColor must be a 7-character hex string like #4C6FA3.");

        user.UpdateProfile(trimmedName, request.AvatarColor, request.AvatarIcon);
        await userRepository.UpdateAsync(user, ct);

        return user.ToDto();
    }
}
