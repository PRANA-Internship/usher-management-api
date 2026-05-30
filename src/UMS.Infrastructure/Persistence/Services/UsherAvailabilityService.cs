using System;
using System.Collections.Generic;
using System.Text;
using UMS.Application.Common.Interfaces;

namespace UMS.Application.Common.Services
{

    public sealed class UsherAvailabilityService(
        IUsherInvitationRepository invitationRepository,
        IUsherScheduleApplicationRepository applicationRepository
    ) : IUsherAvailablityService
    {
        public async Task<bool> IsAvailableAsync(
            Guid usherId,
            DateOnly startDate,
            DateOnly endDate,
            CancellationToken ct = default)
        {
            var invitationTask =
                invitationRepository.HasDateConflictAsync(
                      usherId,
                      startDate,
                      endDate,
                       ct);

            var applicationTask =
                applicationRepository.HasDateConflictAsync(
                    usherId,
                    startDate,
                    endDate,
                    ct);

            await Task.WhenAll(invitationTask, applicationTask);

            var invitationConflict = await invitationTask;
            var applicationConflict = await applicationTask;
            return !invitationConflict && !applicationConflict;
        }
    }
}
