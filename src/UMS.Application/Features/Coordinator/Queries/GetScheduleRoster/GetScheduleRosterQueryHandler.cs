using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Application.Common.Interfaces;
using UMS.Contracts.Coordinator;
using UMS.Domain.Common;
using UMS.Domain.Enums;

namespace UMS.Application.Features.Coordinator.Queries.GetScheduleRoster
{
    public sealed class GetScheduleRosterQueryHandler(
    IUsherScheduleApplicationRepository applicationRepository,
    IUsherInvitationRepository invitationRepository,
    IUsherRepository usherRepository
) : IRequestHandler<GetScheduleRosterQuery, Result<object>>
    {
        public async Task<Result<object>> Handle(
            GetScheduleRosterQuery query,
            CancellationToken cancellationToken)
        {
            switch (query.Filter)
            {
                case CoordinatorScheduleFilter.PendingApplication:
                case CoordinatorScheduleFilter.DeclinedApplication:
                    {
                        var status = query.Filter == CoordinatorScheduleFilter.PendingApplication
                            ? InvitationStatus.PENDING
                            : InvitationStatus.DECLINED;

                        var (items, total) = await applicationRepository
                            .GetBySchedulePagedAsync(
                                query.ExternalScheduleId, status,
                                query.Page, query.Size, cancellationToken);

                        var mapped = items.Select(a => new ScheduleApplicationItem(
                            ApplicationId: a.Id,
                            UsherId: a.UsherId,
                            FullName: a.Usher.User!.FullName,
                            Email: a.Usher.User.Email,
                            Phone: a.Usher.User.Phone,
                            City: a.Usher.City,
                            AppliedAt: a.CreatedAt,
                            Status: a.Status.ToString()
                        )).ToList();

                        var totalPages = (int)Math.Ceiling(total / (double)query.Size);

                        return Result<object>.Success(new PagedApplicationResponse(
                            Items: mapped,
                            TotalCount: total,
                            Page: query.Page,
                            Size: query.Size,
                            TotalPages: totalPages
                        ));
                    }

                case CoordinatorScheduleFilter.PendingInvitation:
                case CoordinatorScheduleFilter.DeclinedInvitation:
                    {
                        var status = query.Filter == CoordinatorScheduleFilter.PendingInvitation
                            ? InvitationStatus.PENDING
                            : InvitationStatus.DECLINED;

                        var (items, total) = await invitationRepository
                            .GetBySchedulePagedAsync(
                                query.ExternalScheduleId, status,
                                query.Page, query.Size, cancellationToken);

                        var mapped = items.Select(i => new ScheduleInvitationItem(
                            InvitationId: i.Id,
                            UsherId: i.UsherId,
                            FullName: i.Usher.User!.FullName,
                            Email: i.Usher.User.Email,
                            Phone: i.Usher.User.Phone,
                            City: i.Usher.City,
                            SentAt: i.CreatedAt,
                            Status: i.Status.ToString()
                        )).ToList();

                        var totalPages = (int)Math.Ceiling(total / (double)query.Size);

                        return Result<object>.Success(new PagedInvitationResponse(
                            Items: mapped,
                            TotalCount: total,
                            Page: query.Page,
                            Size: query.Size,
                            TotalPages: totalPages
                        ));
                    }

                default:
                    return Result<object>.Success(
                        new PagedApplicationResponse([], 0, query.Page, query.Size, 0));
            }
        }
    }
}
