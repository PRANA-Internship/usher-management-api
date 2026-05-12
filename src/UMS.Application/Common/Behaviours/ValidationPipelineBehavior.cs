using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Domain.Common;

namespace UMS.Application.Common.Behaviours
{

    public sealed class ValidationPipelineBehavior<TRequest, TResponse>(
        IEnumerable<IValidator<TRequest>> validators
    ) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : Result
    {
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (!validators.Any())
                return await next();

            var context = new ValidationContext<TRequest>(request);
            var failures = validators
                .Select(v => v.Validate(context))
                .SelectMany(r => r.Errors)
                .Where(f => f is not null)
                .ToList();

            if (failures.Count == 0)
                return await next();

            // Aggregate all validation failures into a single structured Error
            var description = string.Join("; ", failures.Select(f => f.ErrorMessage));
            var error = new Error("VALIDATION_ERROR", description);

            // Dynamically invoke Result<T>.Failure(error) via the static factory
            var failureMethod = typeof(TResponse)
                .GetMethod(nameof(Result.Failure), [typeof(Error)])!;

            return (TResponse)failureMethod.Invoke(null, [error])!;
        }
    }
}
