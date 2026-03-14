using WorkCale.Application.DTOs;
using MediatR;

namespace WorkCale.Application.Features.Auth;

public record RegisterCommand(string Email, string DisplayName, string Password) : IRequest<AuthResult>;
