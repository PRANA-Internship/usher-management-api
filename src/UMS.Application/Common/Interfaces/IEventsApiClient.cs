using System;
using System.Collections.Generic;
using System.Text;

using UMS.Application.Common.Models;

namespace UMS.Application.Common.Interfaces
{
    public interface IEventsApiClient
    {
        Task<IReadOnlyList<ExternalEventDto>> GetEventsAsync(CancellationToken ct = default);
        Task<ExternalEventDetailDto?> GetEventByIdAsync(string eventId, CancellationToken ct = default);
        Task<ScheduleDto?> GetScheduleByIdAsync(string eventId, string scheduleId, CancellationToken ct = default);
    }
}