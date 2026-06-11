

using System;
using System.Collections.Generic;
using System.Text;

using MediatR;

using UMS.Application.Common.Interfaces;
using UMS.Contracts.Auth;
using UMS.Domain.Common;
using UMS.Domain.Entities;
using UMS.Domain.Enums;
using UMS.Infrastructure.Cache;

using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Auth.Commands.ApproveApplication
{

    public sealed class ApproveUsherApplicationCommandHandler(
        IUsherRepository usherRepository,
        IUserRepository userRepository,
        IEmailVerificationTokenRepository tokenRepository,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        IUsherScheduleApplicationRepository applicationRepository,
        IEventsApiClient eventsApiClient
    ) : IRequestHandler<ApproveUsherApplicationCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(
            ApproveUsherApplicationCommand command,
            CancellationToken cancellationToken)
        {
            var usher = await usherRepository.GetByIdAsync(command.UsherId, cancellationToken);
            if (usher is null)
                return UsherErrors.NotFound;

            if (usher.ApprovalStatus == ApprovalStatus.APPROVED)
                return UsherErrors.AlreadyApproved;

            var user = await userRepository.GetByIdAsync(usher.UserId, cancellationToken);
            if (user is null)
                return UsherErrors.NotFound;

            // Generate password setup token
            var verificationToken = EmailVerificationToken.Create(
                userId: user.Id,
                tokenType: TokenType.EmailVerification,
                validFor: TimeSpan.FromHours(48));

            await unitOfWork.ExecuteInTransactionAsync(async () =>
  {
      usher.ApproveUsher(command.AdminId);
      await usherRepository.UpdateAsync(usher, cancellationToken);

      user.SetRole(UserRole.USHER);
      await userRepository.UpdateAsync(user, cancellationToken);

      await tokenRepository.AddAsync(verificationToken, cancellationToken);

      if (!string.IsNullOrWhiteSpace(usher.PendingEventId) &&
          !string.IsNullOrWhiteSpace(usher.PendingScheduleId))
      {
          try
          {
              var schedule = await eventsApiClient.GetScheduleByIdAsync(
                  usher.PendingEventId,
                  usher.PendingScheduleId,
                  cancellationToken);

              if (schedule is not null)
              {
                  var application = UsherScheduleApplication.Create(
                      externalScheduleId: usher.PendingScheduleId,
                      externalEventId: usher.PendingEventId,
                      usherId: usher.Id,
                      startDate: DateOnly.Parse(schedule.StartDate),
                      endDate: DateOnly.Parse(schedule.EndDate));

                  await applicationRepository.AddAsync(application, cancellationToken);

              }
          }
          catch (Exception)
          {

          }
      }

  }, cancellationToken);


            await cache.RemoveAsync(CacheKeys.AdminAttendanceTrend, cancellationToken);
            // Send password setup email outside transaction
            try
            {
                await emailService.SendPasswordSetupAsync(
                    toEmail: user.Email,
                    fullName: user.FullName,
                    token: verificationToken.Token,
                    ct: cancellationToken);
            }
            catch
            {
            }

            return Result<Guid>.Success(usher.Id);


        }
    }

}