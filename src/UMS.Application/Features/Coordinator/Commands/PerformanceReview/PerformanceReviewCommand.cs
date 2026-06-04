using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Domain.Common;
using UMS.Contracts.Coordinator.Performance;

namespace UMS.Application.Features.Coordinator.Commands.PerformanceReview
{
    public sealed record SubmitPerformanceReviewCommand(
     Guid CoordinatorId,
     string ExternalEventId,
     string ExternalScheduleId,
     Guid UsherId,
     int Grooming,
     int Professionalism,
     int Communication,
     int Teamwork,
     int OverallScore,
     string? Remarks
 ) : IRequest<Result<PerformanceReviewResponse>>;

    public sealed class SubmitPerformanceReviewValidator
        : AbstractValidator<SubmitPerformanceReviewCommand>
    {
        private const int Min = 1;
        private const int Max = 5;
        public SubmitPerformanceReviewValidator()
        {
            RuleFor(x => x.CoordinatorId).NotEmpty();
            RuleFor(x => x.ExternalEventId).NotEmpty();
            RuleFor(x => x.ExternalScheduleId).NotEmpty();
            RuleFor(x => x.UsherId).NotEmpty();

            RuleFor(x => x.Grooming)
                .InclusiveBetween(Min, Max)
                .WithMessage("Grooming rating must be between 1 and 5.");
            RuleFor(x => x.Professionalism)
                .InclusiveBetween(Min, Max)
                .WithMessage("Professionalism rating must be between 1 and 5.");
            RuleFor(x => x.Communication)
                .InclusiveBetween(Min, Max)
                .WithMessage("Communication rating must be between 1 and 5.");
            RuleFor(x => x.Teamwork)
                .InclusiveBetween(Min, Max)
                .WithMessage("Teamwork rating must be between 1 and 5.");
            RuleFor(x => x.OverallScore)
                .InclusiveBetween(Min, Max)
                .WithMessage("Overall score must be between 1 and 5.");

            When(x => x.Remarks is not null, () =>
                RuleFor(x => x.Remarks).MaximumLength(1000));
        }
    }

}
