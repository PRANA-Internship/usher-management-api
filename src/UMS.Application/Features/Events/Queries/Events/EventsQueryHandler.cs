using System;
using System.Collections.Generic;
using System.Text;

using MediatR;

using UMS.Application.Common.Interfaces;
using UMS.Contracts.Events;
using UMS.Domain.Common;

using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Events.Queries.GetEvents
{

    public sealed record GetEventsQuery(int PageNumber, int PageSize) : IRequest<Result<PaginatedEventsResponse>>;
    public sealed class EventsQueryHandler(
         IEventsApiClient eventsApiClient
     ) : IRequestHandler<GetEventsQuery, Result<PaginatedEventsResponse>>
    {
        public async Task<Result<PaginatedEventsResponse>> Handle(
            GetEventsQuery query,
            CancellationToken cancellationToken)

        {
            try
            {

                var paginatedEvents = await eventsApiClient.GetPaginatedEventsAsync(query.PageNumber, query.PageSize, cancellationToken);

                var items = paginatedEvents.Events.Select(e => new EventSummaryResponse(
                    EventId: e.EventId,
                    EventName: e.EventName,
                    EventLogoLightUrl: e.EventLogoLightUrl,
                    EventLogoDarkUrl: e.EventLogoDarkUrl
                )).ToList();

                var response = new PaginatedEventsResponse(
                    items, paginatedEvents.PageNumber, paginatedEvents.PageSize, paginatedEvents.HasNextPage, paginatedEvents.HasPreviousPage);

                return Result<PaginatedEventsResponse>.Success(response);
            }
            catch
            {
                return ScheduleErrors.ExternalApiFailed;
            }
        }
    }
}