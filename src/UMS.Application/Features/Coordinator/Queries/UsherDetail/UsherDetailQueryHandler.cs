using System;
using System.Collections.Generic;
using System.Text;

using MediatR;

using UMS.Application.Common.Interfaces;
using UMS.Contracts.Coordinator.Usher;
using UMS.Domain.Common;

using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Coordinator.Queries.UsherDetail
{

    public sealed class UsherDetailQueryHandler(
        IUsherRepository usherRepository,
        IUsherAttendanceRepository attendanceRepository,
        IUsherScheduleApplicationRepository applicationRepository,
        IUsherInvitationRepository invitationRepository,
        IUsherPerformanceReviewRepository reviewRepository
    ) : IRequestHandler<UsherDetailQuery, Result<UsherDetailResponse>>
    {
        public async Task<Result<UsherDetailResponse>> Handle(
            UsherDetailQuery query,
            CancellationToken cancellationToken)
        {
            var usher = await usherRepository
                .GetByIdAsync(query.UsherId, cancellationToken);

            if (usher is null)
                return UsherErrors.NotFound;


            var confirmedByApp = await applicationRepository
                .GetConfirmedByUsherAsync(usher.Id, cancellationToken);

            var confirmedByInvite = await invitationRepository
                .GetAcceptedByUsherAsync(usher.Id, cancellationToken);

            var attendanceRecords = await attendanceRepository
                .GetRawByUsherAsync(usher.Id, cancellationToken);

            var totalSessions = attendanceRecords.Count;
            var totalScore = attendanceRecords
                .Sum(a => (int)a.Status < 0 ? 0 : (int)a.Status);
            var attendanceRate = totalSessions > 0
                ? Math.Round((double)totalScore / (totalSessions * 2) * 100, 2)
                : 0;

            var reviews = await reviewRepository
                .GetByUsherAsync(usher.Id, cancellationToken);

            var averageRating = reviews.Count > 0
                ? Math.Round(reviews.Average(r =>
                    (r.Grooming + r.Professionalism +
                     r.Communication + r.Teamwork +
                     r.OverallScore) / 5.0), 2)
                : 0;

            return Result<UsherDetailResponse>.Success(new UsherDetailResponse(
                UsherId: usher.Id,
                FullName: usher.User!.FullName,
                Email: usher.User.Email,
                Phone: usher.User.Phone,
                TotalConfirmedEvents: confirmedByApp.Count + confirmedByInvite.Count,
                OverallAttendanceRate: attendanceRate,
                AveragePerformanceRating: averageRating
            ));
        }
    }
}