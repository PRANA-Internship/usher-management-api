using System;
using System.Threading;
using System.Threading.Tasks;

using MediatR;

using UMS.Application.Common;
using UMS.Application.Common.Interfaces;
using UMS.Application.Common.Models;
using UMS.Contracts.Events;
using UMS.Domain.Common;

using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Events.Commands.RemoveCoordinator
{
    public sealed class RemoveCoordinatorCommandHandler : IRequestHandler<RemoveCoordinatorCommand, Result<RemoveCoordinatorResponse>>
    {
        private readonly IScheduleAssignmentRepository _assignmentRepository;
        private readonly ICacheService _cache;
        private readonly IUnitOfWork _unitOfWork;

        public RemoveCoordinatorCommandHandler(
            IScheduleAssignmentRepository assignmentRepository,
            ICacheService cache,
            IUnitOfWork unitOfWork)
        {
            _assignmentRepository = assignmentRepository;
            _cache = cache;
            _unitOfWork = unitOfWork;
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
            await _cache.RemoveAsync(CacheKeys.CoordinatorAnalytics(assignment.CoordinatorId), cancellationToken);

            var response = new RemoveCoordinatorResponse(
                AssignmentId: assignment.Id,
                ExternalScheduleId: assignment.ExternalScheduleId,
                ExternalEventId: assignment.ExternalEventId);

            return Result<RemoveCoordinatorResponse>.Success(response);
        }
    }
}