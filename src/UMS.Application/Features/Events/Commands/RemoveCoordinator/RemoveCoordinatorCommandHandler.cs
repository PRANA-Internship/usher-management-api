using System;
using System.Threading.Tasks;
using UMS.Application.Common.Interfaces;
using UMS.Application.Common.Models;
using UMS.Contracts.Events;
using UMS.Domain.Common;
using MediatR;
using System;
using System.Threading;
using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Events.Commands.RemoveCoordinator
{
    public sealed class RemoveCoordinatorCommandHandler : IRequestHandler<RemoveCoordinatorCommand, Result<RemoveCoordinatorResponse>>
    {
        private readonly IScheduleAssignmentRepository _assignmentRepository;
        private readonly ICacheService _cache;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventsApiClient _eventsApiClient;

        public RemoveCoordinatorCommandHandler(
            IScheduleAssignmentRepository assignmentRepository,
            ICacheService cache,
            IUnitOfWork unitOfWork,
            IEventsApiClient eventsApiClient)
        {
            _assignmentRepository = assignmentRepository;
            _cache = cache;
            _unitOfWork = unitOfWork;
            _eventsApiClient = eventsApiClient;
        }

        public async Task<Result<RemoveCoordinatorResponse>> Handle(
            RemoveCoordinatorCommand command,
            CancellationToken cancellationToken)
        {
            var assignment = await _assignmentRepository.GetByScheduleIdAsync(
                command.ExternalScheduleId, cancellationToken);

            if (assignment is null)
                return ScheduleErrors.AssignmentNotFound;

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await _assignmentRepository.DeleteAsync(assignment, cancellationToken);
            }, cancellationToken);

            await _cache.RemoveAsync(command.ExternalEventId, cancellationToken);

            var response = new RemoveCoordinatorResponse(
                AssignmentId: assignment.Id,
                ExternalScheduleId: assignment.ExternalScheduleId,
                ExternalEventId: assignment.ExternalEventId);

            return Result<RemoveCoordinatorResponse>.Success(response);
        }
    }
}