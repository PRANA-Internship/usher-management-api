using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Application.Common.Interfaces;
using UMS.Domain.Common;
using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Ushers.Command
{
    public sealed class UpdateUsherProfileCommandHandler(
        IUsherRepository usherRepository,
        IUserRepository userRepository,
        IFileStorageService fileStorage,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<UpdateUsherProfileCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(
            UpdateUsherProfileCommand command,
            CancellationToken cancellationToken)
        {
            var usher = await usherRepository.GetByUserIdAsync(command.UserId, cancellationToken);
            if (usher is null)
                return UsherErrors.NotFound;

            var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken);
            if (user is null)
                return UsherErrors.NotFound;

            string? newPhotoUrl = null;
            if (command.ProfilePhoto is not null)
            {
                try
                {
                    newPhotoUrl = await fileStorage.UploadFileAsync(
                        fileStream: command.ProfilePhoto.OpenReadStream(),
                        fileName: command.ProfilePhoto.FileName,
                        contentType: command.ProfilePhoto.ContentType,
                        folder: "profile-photos",
                        ct: cancellationToken);
                }
                catch
                {
                    return UsherErrors.FileUploadFailed;
                }
            }

            var oldPhotoUrl = newPhotoUrl is not null ? usher.ProfilePhotoUrl : null;

            try
            {
                await unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    if (command.Phone is not null)
                    {
                        user.UpdatePhone(command.Phone);
                        await userRepository.UpdateAsync(user, cancellationToken);
                    }

                    usher.UpdateProfile(
                        address: command.Address,
                        city: command.City,
                        emergencyContactName: command.EmergencyContactName,
                        emergencyContactPhone: command.EmergencyContactPhone,
                        educationLevel: command.EducationLevel,
                        experienceSummary: command.ExperienceSummary,
                        languages: command.Languages,
                        sector: command.Sector);


                    if (newPhotoUrl is not null)
                        usher.UpdateProfilePhoto(newPhotoUrl);

                    await usherRepository.UpdateAsync(usher, cancellationToken);

                }, cancellationToken);
            }
            catch
            {
                if (newPhotoUrl is not null)
                    try { await fileStorage.DeleteAsync(newPhotoUrl, cancellationToken); } catch { }

                return UsherErrors.ApplicationSaveFailed;
            }

            if (oldPhotoUrl is not null)
                try { await fileStorage.DeleteAsync(oldPhotoUrl, cancellationToken); } catch { }

            return Result<bool>.Success(true);
        }
    }











}


