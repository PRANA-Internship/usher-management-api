using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Contracts.Coordinator.Performance
{
    public sealed record PerformanceReviewRequest(
      Guid UsherId,
      string eventId,
      string scheduleId,
      int Grooming,
      int Professionalism,
      int Communication,
      int Teamwork,
      int OverallScore,
      string? Remarks
  );
    public sealed record PerformanceReviewResponse(
    Guid ReviewId,
    Guid UsherId,
    string FullName,
    string ExternalScheduleId,
    int Grooming,
    int Professionalism,
    int Communication,
    int Teamwork,
    int OverallScore,
    double AverageRating,
    string? Remarks,
    DateTimeOffset SubmittedAt
);




}
