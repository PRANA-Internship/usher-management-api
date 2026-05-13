using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Application.Common.Interfaces;
using UMS.Contracts.Usher;
using UMS.Domain.Common;
using UMS.Domain.Entities;
using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Auth.Commands.SubmitApplication
{
    public sealed class SubmitUsherApplicationCommandHandler(
    IUserRepository userRepository,
    IUsherRepository usherRepository,
    IFileStorageService fileStorage,
    IUnitOfWork unitOfWork
) : IRequestHandler<SubmitUsherApplicationCommand, Result<SubmitUsherApplicationResponse>>
    {
        public async Task<Result<SubmitUsherApplicationResponse>> Handle(
            SubmitUsherApplicationCommand request,
            CancellationToken cancellationToken)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            if (await userRepository.ExistsByEmailAsync(email, cancellationToken))
                return UsherErrors.EmailAlreadyExists;

            // Upload files first — if either fails we never touch the db
            string profilePhotoPath;
            string idDocumentPath;

            try
            {
                profilePhotoPath = await fileStorage.UploadFileAsync(
                    fileStream: request.ProfilePhoto.OpenReadStream(),
                    fileName: request.ProfilePhoto.FileName,
                    contentType: request.ProfilePhoto.ContentType,
                    folder: "profile-photos",
                    ct: cancellationToken);

                idDocumentPath = await fileStorage.UploadFileAsync(
                    fileStream: request.IdDocument.OpenReadStream(),
                    fileName: request.IdDocument.FileName,
                    contentType: request.IdDocument.ContentType,
                    folder: "id-documents",
                    ct: cancellationToken);
            }
            catch (Exception)
            {
                return UsherErrors.FileUploadFailed;
            }

            // below code Wrap DB operations in a transaction

            try
            {
                await unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var guestData = new CreateGuest(
                             request.FullName,
                             request.Email,
                             request.Phone);

                    var user = User.CreateGuest(guestData);
                    await userRepository.AddAsync(user, cancellationToken);
                    var UsherData = new CreateUsherData(
                        Gender: request.Gender,
                        DateOfBirth: request.DateOfBirth,
                        Address: request.Address,
                        City: request.City,
                        EmergencyContactName: request.EmergencyContactName,
                        EmergencyContactPhone: request.EmergencyContactPhone,
                        EducationLevel: request.EducationLevel,
                        ExperienceSummary: request.ExperienceSummary,
                        Languages: request.Languages,
                        Sector: request.Sector,
                        ProfilePhotoUrl: profilePhotoPath,
                        IdDocumentUrl: idDocumentPath);

                    var usher = Usher.CreateApplication(user.Id, UsherData);
                    await usherRepository.AddAsync(usher, cancellationToken);
                }, cancellationToken);

                return Result<SubmitUsherApplicationResponse>.Success(new SubmitUsherApplicationResponse(

                    FullName: request.FullName
                ));
            }
            catch (Exception)
            {
                // Best-effort cleanup of uploaded files
                await TryDeleteFilesAsync(profilePhotoPath, idDocumentPath, cancellationToken);

                return UsherErrors.ApplicationSaveFailed;
            }

        }

        private async Task TryDeleteFilesAsync(string photoPath, string docPath, CancellationToken ct)
        {
            try { await fileStorage.DeleteAsync(photoPath, ct); } catch { /* ignore */ }
            try { await fileStorage.DeleteAsync(docPath, ct); } catch { /* ignore */ }
        }
    }
}
