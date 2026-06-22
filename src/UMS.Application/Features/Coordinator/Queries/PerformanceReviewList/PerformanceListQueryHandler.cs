using System;
using System.Collections.Generic;
using System.Text;

using MediatR;

using UMS.Application.Common.Interfaces;
using UMS.Contracts.Coordinator.Performance;
using UMS.Domain.Common;
using UMS.Domain.Entities;

using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Coordinator.Queries.PerformanceReviewList
{
    public sealed class GetPerformanceReviewListQueryHandler(
    IScheduleAssignmentRepository assignmentRepository,
    IUsherPerformanceReviewRepository reviewRepository,
    IUsherScheduleApplicationRepository applicationRepository,
    IUsherInvitationRepository invitationRepository
) : IRequestHandler<PerformanceReviewListQuery, Result<PerformanceReviewListResponse>>
    {
        public async Task<Result<PerformanceReviewListResponse>> Handle(
            PerformanceReviewListQuery query,
            CancellationToken cancellationToken)
        {
            var assignment = await assignmentRepository
                .GetByScheduleIdAsync(query.ExternalScheduleId, cancellationToken);

            if (assignment is null || assignment.CoordinatorId != query.CoordinatorId)
                return PerformanceReviewErrors.NotYourSchedule;

            var reviews = await reviewRepository
                .GetByScheduleAsync(query.ExternalScheduleId, cancellationToken);

            var reviewMap = reviews.ToDictionary(r => r.UsherId);

            var confirmedApps = await applicationRepository
                .GetApprovedBySchedulePagedAsync(
                    query.ExternalScheduleId, 0, int.MaxValue, cancellationToken);

            var confirmedInvites = await invitationRepository
                .GetAcceptedBySchedulePagedAsync(
                    query.ExternalScheduleId, 0, int.MaxValue, cancellationToken);

            var usherItems = new List<PerformanceReviewItem>();
            var seenIds = new HashSet<Guid>();

            foreach (var app in confirmedApps)
            {
                if (!seenIds.Add(app.UsherId)) continue;

                reviewMap.TryGetValue(app.UsherId, out var review);

                usherItems.Add(BuildItem(
                    app.Usher.Id,
                    app.Usher.User!.FullName,
                    app.Usher.User.Phone,
                    app.Usher.City,
                    review));
            }

            foreach (var invite in confirmedInvites)
            {
                if (!seenIds.Add(invite.UsherId)) continue;

                reviewMap.TryGetValue(invite.UsherId, out var review);

                usherItems.Add(BuildItem(
                    invite.Usher.Id,
                    invite.Usher.User!.FullName,
                    invite.Usher.User.Phone,
                    invite.Usher.City,
                    review));
            }

            var sorted = usherItems
                .OrderByDescending(u => u.IsReviewed)
                .ThenBy(u => u.FullName)
                .ToList();

            var totalReviewed = sorted.Count(u => u.IsReviewed);

            return Result<PerformanceReviewListResponse>.Success(
                new PerformanceReviewListResponse(
                    ExternalScheduleId: query.ExternalScheduleId,
                    ExternalEventId: query.ExternalEventId,
                    TotalConfirmed: sorted.Count,
                    TotalReviewed: totalReviewed,
                    TotalPending: sorted.Count - totalReviewed,
                    Ushers: sorted
                ));
        }

        private static PerformanceReviewItem BuildItem(
            Guid usherId,
            string fullName,
            string phone,
            string city,
            UsherPerformanceReview? review) =>
            new(
                UsherId: usherId,
                FullName: fullName,
                Phone: phone,
                City: city,
                IsReviewed: review is not null,
                ReviewId: review?.Id,
                Grooming: review?.Grooming,
                Professionalism: review?.Professionalism,
                Communication: review?.Communication,
                Teamwork: review?.Teamwork,
                OverallScore: review?.OverallScore,
                AverageRating: review?.AverageRating,
                Remarks: review?.Remarks,
                SubmittedAt: review?.SubmittedAt
            );
    }
}