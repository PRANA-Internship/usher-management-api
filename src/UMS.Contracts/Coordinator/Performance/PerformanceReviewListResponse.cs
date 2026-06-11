using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Contracts.Coordinator.Performance
{
    public sealed record PerformanceReviewItem(
    Guid UsherId,
    string FullName,
    string Phone,
    string City,
    bool IsReviewed,
    Guid? ReviewId,
    int? Grooming,
    int? Professionalism,
    int? Communication,
    int? Teamwork,
    int? OverallScore,
    double? AverageRating,
    string? Remarks,
    DateTimeOffset? SubmittedAt
);

    public sealed record PerformanceReviewListResponse(
       string ExternalScheduleId,
       string ExternalEventId,
       int TotalConfirmed,
       int TotalReviewed,
       int TotalPending,
       IReadOnlyList<PerformanceReviewItem> Ushers
   );

}