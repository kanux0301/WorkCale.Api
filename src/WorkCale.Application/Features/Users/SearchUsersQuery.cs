using WorkCale.Application.DTOs;
using MediatR;

namespace WorkCale.Application.Features.Users;

public record SearchUsersQuery(Guid RequestingUserId, string Query) : IRequest<IEnumerable<UserDto>>;
