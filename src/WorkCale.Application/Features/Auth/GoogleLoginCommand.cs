using WorkCale.Application.DTOs;
using MediatR;

namespace WorkCale.Application.Features.Auth;

public record GoogleLoginCommand(string IdToken) : IRequest<AuthResult>;
