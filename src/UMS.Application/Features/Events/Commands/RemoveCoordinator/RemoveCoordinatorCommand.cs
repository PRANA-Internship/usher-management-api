using MediatR;
using System;
using UMS.Contracts.Events;
using UMS.Domain.Common;
namespace UMS.Application.Features.Events.Commands.RemoveCoordinator;

public sealed record RemoveCoordinatorCommand(
    string ExternalEventId,
    string ExternalScheduleId,
    Guid AdminId
) : IRequest<Result<RemoveCoordinatorResponse>>;
