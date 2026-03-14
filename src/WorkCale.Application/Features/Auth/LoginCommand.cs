using WorkCale.Application.DTOs;
using MediatR;

namespace WorkCale.Application.Features.Auth;

public record LoginCommand(string Email, string Password) : IRequest<AuthResult>;
