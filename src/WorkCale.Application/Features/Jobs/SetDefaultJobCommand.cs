using MediatR;

namespace WorkCale.Application.Features.Jobs;

public record SetDefaultJobCommand(Guid JobId, Guid UserId) : IRequest;
