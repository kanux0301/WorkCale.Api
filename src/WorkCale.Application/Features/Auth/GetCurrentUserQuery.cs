using WorkCale.Application.DTOs;
using MediatR;

namespace WorkCale.Application.Features.Auth;

public record GetCurrentUserQuery(Guid UserId) : IRequest<UserDto>;
