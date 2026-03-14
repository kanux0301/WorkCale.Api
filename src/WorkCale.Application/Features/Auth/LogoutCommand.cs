using MediatR;

namespace WorkCale.Application.Features.Auth;

public record LogoutCommand(string RefreshToken) : IRequest;
