using System;
using System.Collections.Generic;
using System.Text;

using MediatR;

using UMS.Application.Common.Interfaces;
using UMS.Application.Common.Models;
using UMS.Contracts.Coordinator.Performance;
using UMS.Domain.Common;
using UMS.Domain.Entities;
using UMS.Infrastructure.Cache;

using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Coordinator.Commands.PerformanceReview
{
    public sealed class PerformanceReviewCommandHandler(
     IScheduleAssignmentRepository assignmentRepository,
     IUsherPerformanceReviewRepository reviewRepository,
     IUsherScheduleApplicationRepository applicationRepository,
     IUsherInvitationRepository invitationRepository,
     IUsherRepository usherRepository,
     IEventsApiClient eventsApiClient,
     IUnitOfWork unitOfWork,
     ICacheService cache
 ) : IRequestHandler<SubmitPerformanceReviewCommand, Result<PerformanceReviewResponse>>
    {
        public async Task<Result<PerformanceReviewResponse>> Handle(
            SubmitPerformanceReviewCommand command,
            CancellationToken cancellationToken)
        {
            var assignment = await assignmentRepository
                .GetByScheduleIdAsync(command.ExternalScheduleId, cancellationToken);

            if (assignment is null || assignment.CoordinatorId != command.CoordinatorId)
                return PerformanceReviewErrors.NotYourSchedule;

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
            var startDate = DateOnly.Parse(schedule.StartDate);
            var endDate = DateOnly.Parse(schedule.EndDate);

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            // Submit date must start between the schedule start and end dates
            if (today < startDate || today > endDate)
                return PerformanceReviewErrors.ScheduleNotEnded;

            var usher = await usherRepository
                .GetByIdAsync(command.UsherId, cancellationToken);

            if (usher is null)
                return PerformanceReviewErrors.UsherNotConfirmed;

            var isConfirmedByApp = await applicationRepository.ExistsAsync(
                command.ExternalScheduleId, usher.Id, cancellationToken);

            var isConfirmedByInvite = await invitationRepository
                .ExistsAsync(command.ExternalScheduleId, usher.Id, cancellationToken);

            if (!isConfirmedByApp && !isConfirmedByInvite)
                return PerformanceReviewErrors.UsherNotConfirmed;

            var alreadyExists = await reviewRepository
                .ExistsAsync(usher.Id, command.ExternalScheduleId, cancellationToken);

            if (alreadyExists)
                return PerformanceReviewErrors.AlreadyReviewed;

            var review = UsherPerformanceReview.Create(
                externalScheduleId: command.ExternalScheduleId,
                externalEventId: command.ExternalEventId,
                usherId: usher.Id,
                coordinatorId: command.CoordinatorId,
                grooming: command.Grooming,
                professionalism: command.Professionalism,
                communication: command.Communication,
                teamwork: command.Teamwork,
                overallScore: command.OverallScore,
                remarks: command.Remarks);

            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await reviewRepository.AddAsync(review, cancellationToken);
            }, cancellationToken);

            await cache.RemoveAsync(CacheKeys.AdminAttendanceTrend, cancellationToken);
            return Result<PerformanceReviewResponse>.Success(
                new PerformanceReviewResponse(
                    ReviewId: review.Id,
                    UsherId: command.UsherId,
                    FullName: usher.User!.FullName,
                    ExternalScheduleId: review.ExternalScheduleId,
                    Grooming: review.Grooming,
                    Professionalism: review.Professionalism,
                    Communication: review.Communication,
                    Teamwork: review.Teamwork,
                    OverallScore: review.OverallScore,
                    AverageRating: review.AverageRating,
                    Remarks: review.Remarks,
                    SubmittedAt: review.SubmittedAt
                ));
        }
    }
}