using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Domain.Entities
{


    public class UsherPerformanceReview : BaseEntity
    {
        public string ExternalScheduleId { get; private set; } = string.Empty;
        public string ExternalEventId { get; private set; } = string.Empty;
        public Guid UsherId { get; private set; }
        public Guid ReviewedByCoordinatorId { get; private set; }

        // Ratings all out of 5
        public int Grooming { get; private set; }
        public int Professionalism { get; private set; }
        public int Communication { get; private set; }
        public int Teamwork { get; private set; }
        public int OverallScore { get; private set; }
        public string? Remarks { get; private set; }
        public DateTimeOffset SubmittedAt { get; private set; }

        public double AverageRating =>
            Math.Round((Grooming + Professionalism + Communication +
                        Teamwork + OverallScore) / 5.0, 2);

        public Usher Usher { get; private set; } = null!;

        private UsherPerformanceReview() { }

        public static UsherPerformanceReview Create(
            string externalScheduleId,
            string externalEventId,
            Guid usherId,
            Guid coordinatorId,
            int grooming,
            int professionalism,
            int communication,
            int teamwork,
            int overallScore,
            string? remarks)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(externalScheduleId);
            ArgumentException.ThrowIfNullOrWhiteSpace(externalEventId);
            if (usherId == Guid.Empty) throw new ArgumentException("UsherId is required.");
            if (coordinatorId == Guid.Empty) throw new ArgumentException("CoordinatorId is required.");

            ValidateRating(grooming, nameof(grooming));
            ValidateRating(professionalism, nameof(professionalism));
            ValidateRating(communication, nameof(communication));
            ValidateRating(teamwork, nameof(teamwork));
            ValidateRating(overallScore, nameof(overallScore));

            return new UsherPerformanceReview
            {
                Id = Guid.NewGuid(),
                ExternalScheduleId = externalScheduleId,
                ExternalEventId = externalEventId,
                UsherId = usherId,
                ReviewedByCoordinatorId = coordinatorId,
                Grooming = grooming,
                Professionalism = professionalism,
                Communication = communication,
                Teamwork = teamwork,
                OverallScore = overallScore,
                Remarks = remarks?.Trim(),
                SubmittedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow
            };
        }

        private static void ValidateRating(int value, string field)
        {
            if (value < 1 || value > 5)
                throw new ArgumentException($"{field} must be between 1 and 5.");
        }
    }
}