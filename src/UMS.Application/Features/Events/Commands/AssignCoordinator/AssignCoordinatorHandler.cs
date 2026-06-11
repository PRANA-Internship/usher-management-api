using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MediatR;

using UMS.Application.Common.Interfaces;
using UMS.Application.Common.Models;
using UMS.Contracts.Events;
using UMS.Domain.Common;
using UMS.Domain.Entities;
using UMS.Domain.Enums;

using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Events.Commands.AssignCoordinator
{

    public sealed class AssignCoordinatorCommandHandler(
        IEventsApiClient eventsApiClient,
        IUserRepository userRepository,
        IScheduleAssignmentRepository assignmentRepository,
        ICacheService cache,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<AssignCoordinatorCommand, Result<AssignCoordinatorResponse>>
    {
        public async Task<Result<AssignCoordinatorResponse>> Handle(
            AssignCoordinatorCommand command,
            CancellationToken cancellationToken)
        {
            var coordinator = await userRepository.GetByIdAsync(
                command.CoordinatorId, cancellationToken);

            if (coordinator is null)
                return ScheduleErrors.CoordinatorNotFound;

            if (coordinator.Role != UserRole.EVENT_COORDINATOR)
                return ScheduleErrors.InvalidCoordinator;

            ScheduleDto? schedule;
            try
            {
                schedule = await eventsApiClient.GetScheduleByIdAsync(
                    command.ExternalEventId,
                    command.ExternalScheduleId,
                    cancellationToken);
            }
            catch
            {
                return ScheduleErrors.ExternalApiFailed;
            }

            if (schedule is null)
                return ScheduleErrors.ScheduleNotFound;

            var existing = await assignmentRepository.GetByScheduleIdAsync(
                command.ExternalScheduleId, cancellationToken);

            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                if (existing is not null)
                {
                    existing.Reassign(command.CoordinatorId, command.AdminId);
                    await assignmentRepository.UpdateAsync(existing, cancellationToken);
                }
                else
                {
                    var assignment = ScheduleAssignment.Create(
                        externalScheduleId: command.ExternalScheduleId,
                        externalEventId: command.ExternalEventId,
                        coordinatorId: command.CoordinatorId,
                        assignedByAdminId: command.AdminId);

                    await assignmentRepository.AddAsync(assignment, cancellationToken);
                }
            }, cancellationToken);

            await cache.RemoveAsync(command.ExternalEventId, cancellationToken);

            var saved = await assignmentRepository.GetByScheduleIdAsync(
                command.ExternalScheduleId, cancellationToken);


            return Result<AssignCoordinatorResponse>.Success(new AssignCoordinatorResponse(
                AssignmentId: saved!.Id,
                ExternalScheduleId: saved.ExternalScheduleId,
                ExternalEventId: saved.ExternalEventId,
                CoordinatorId: coordinator.Id,
                CoordinatorName: coordinator.FullName,
                CoordinatorEmail: coordinator.Email,
                AssignedAt: saved.AssignedAt
            ));
        }
    }
}