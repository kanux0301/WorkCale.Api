using MediatR;
using WorkCale.Application.DTOs;
using WorkCale.Application.Mappings;
using WorkCale.Application.Services;

namespace WorkCale.Application.Features.Users;

public class SearchUsersQueryHandler(IUserRepository userRepository)
    : IRequestHandler<SearchUsersQuery, IEnumerable<UserDto>>
{
    public async Task<IEnumerable<UserDto>> Handle(SearchUsersQuery request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Query) || request.Query.Length < 2)
            return [];

        var users = await userRepository.SearchAsync(request.Query, ct);
        return users
            .Where(u => u.Id != request.RequestingUserId)
            .Select(u => u.ToDto());
    }
}
