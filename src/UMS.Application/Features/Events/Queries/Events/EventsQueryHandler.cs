using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Application.Common.Interfaces;
using UMS.Contracts.Events;
using UMS.Domain.Common;
using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Events.Queries.GetEvents
{

    public sealed record GetEventsQuery : IRequest<Result<IReadOnlyList<EventSummaryResponse>>>;
    public sealed class EventsQueryHandler(
         IEventsApiClient eventsApiClient
     ) : IRequestHandler<GetEventsQuery, Result<IReadOnlyList<EventSummaryResponse>>>
    {
        public async Task<Result<IReadOnlyList<EventSummaryResponse>>> Handle(
            GetEventsQuery query,
            CancellationToken cancellationToken)

        {
            try
            {

                var events = await eventsApiClient.GetEventsAsync(cancellationToken);

                var response = events.Select(e => new EventSummaryResponse(
                    EventId: e.EventId,
                    EventName: e.EventName,
                    EventLogoLightUrl: e.EventLogoLightUrl,
                    EventLogoDarkUrl: e.EventLogoDarkUrl
                )).ToList();

                return Result<IReadOnlyList<EventSummaryResponse>>.Success(response);
            }
            catch
            {
                return ScheduleErrors.ExternalApiFailed;
            }
        }
    }
}
