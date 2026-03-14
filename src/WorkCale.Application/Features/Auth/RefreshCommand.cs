using WorkCale.Application.DTOs;
using MediatR;

namespace WorkCale.Application.Features.Auth;

public record RefreshCommand(string RefreshToken) : IRequest<AuthResult>;
