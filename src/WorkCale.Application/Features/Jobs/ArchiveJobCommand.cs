using MediatR;

namespace WorkCale.Application.Features.Jobs;

public record ArchiveJobCommand(Guid JobId, Guid UserId) : IRequest;
