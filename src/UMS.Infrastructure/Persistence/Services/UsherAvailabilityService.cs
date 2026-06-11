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
            var invitationConflict = await invitationRepository
    .HasDateConflictAsync(usherId, startDate, endDate, ct);

            var applicationConflict = await applicationRepository
                .HasDateConflictAsync(usherId, startDate, endDate, ct);

            return !invitationConflict && !applicationConflict;
        }
    }
}