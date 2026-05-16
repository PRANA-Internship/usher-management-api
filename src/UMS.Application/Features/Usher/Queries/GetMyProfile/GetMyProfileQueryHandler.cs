using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Application.Common.Interfaces;
using UMS.Domain.Common;
using UMS.Contracts.GetProfile;
using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Ushers.Queries.GetMyProfile
{
    public sealed record GetMyProfileQuery(Guid UserId)
     : IRequest<Result<GetMyProfileUsher>>;
    public sealed class GetMyProfileQueryHandler(
    IUsherRepository usherRepository,
    IFileStorageService fileStorage
) : IRequestHandler<GetMyProfileQuery, Result<GetMyProfileUsher>>
    {
        public async Task<Result<GetMyProfileUsher>> Handle(
            GetMyProfileQuery query,
            CancellationToken cancellationToken)
        {
            var usher = await usherRepository.GetByUserIdAsync(query.UserId, cancellationToken);

            if (usher is null)
                return UsherErrors.NotFound;

            var profilePhotoUrl = await fileStorage.GetPresignedUrlAsync(
                usher.ProfilePhotoUrl, expirySeconds: 3600, ct: cancellationToken);

            var idDocumentUrl = await fileStorage.GetPresignedUrlAsync(
                usher.IdDocumentUrl, expirySeconds: 3600, ct: cancellationToken);

            return Result<GetMyProfileUsher>.Success(new GetMyProfileUsher(
                    UserId: usher.UserId,
                    FullName: usher.User!.FullName,
                    Email: usher.User.Email,
                    Phone: usher.User.Phone,
                    Gender: usher.Gender.ToString(),
                    DateOfBirth: usher.DateOfBirth,
                    Address: usher.Address,
                    City: usher.City,
                    EmergencyContactName: usher.EmergencyContactName,
                    EmergencyContactPhone: usher.EmergencyContactPhone,
                    EducationLevel: usher.EducationLevel,
                    ExperienceSummary: usher.ExperienceSummary,
                    Languages: usher.Languages,
                    Sector: usher.Sector,
                    ProfilePhotoPath: profilePhotoUrl,
                    IdDocumentPath: idDocumentUrl,
                    ApplicationStatus: usher.ApprovalStatus

                ));
        }
    }

}
