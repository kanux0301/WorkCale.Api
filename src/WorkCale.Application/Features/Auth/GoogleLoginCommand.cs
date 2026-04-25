using WorkCale.Application.DTOs;
using MediatR;

namespace WorkCale.Application.Features.Auth;

/// <summary>
/// <paramref name="InviteCode"/> is required only when Google auth creates a brand-new user.
/// Existing accounts ignore the field — the client may pass null for returning users.
/// </summary>
public record GoogleLoginCommand(string IdToken, string? InviteCode) : IRequest<AuthResult>;
