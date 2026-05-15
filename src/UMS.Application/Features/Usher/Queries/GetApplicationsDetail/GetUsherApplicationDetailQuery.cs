using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using UMS.Application.Common.Interfaces;
using UMS.Contracts.Usher;
using UMS.Domain.Common;
using static UMS.Domain.Common.Error;
namespace UMS.Application.Features.Ushers.Queries.GetApplicationsDetail
{

    public sealed record GetUsherApplicationDetailQuery(Guid UsherId)
        : IRequest<Result<GetUsherApplicationDetailResponse>>;

    public sealed class GetUsherApplicationDetailQueryHandler(
        IUsherRepository usherRepository,
           IFileStorageService fileStorage
    ) : IRequestHandler<GetUsherApplicationDetailQuery, Result<GetUsherApplicationDetailResponse>>
    {
        public async Task<Result<GetUsherApplicationDetailResponse>> Handle(
            GetUsherApplicationDetailQuery query,
            CancellationToken cancellationToken)
        {
            var usher = await usherRepository.GetByIdAsync(query.UsherId, cancellationToken);

            if (usher is null)
                return UsherErrors.NotFound;
            var profilePhotoUrl = await fileStorage.GetPresignedUrlAsync(
            usher.ProfilePhotoUrl, expirySeconds: 3600, ct: cancellationToken);

            var idDocumentUrl = await fileStorage.GetPresignedUrlAsync(
                usher.IdDocumentUrl, expirySeconds: 3600, ct: cancellationToken);


            return Result<GetUsherApplicationDetailResponse>.Success(
                new GetUsherApplicationDetailResponse(
                    UsherId: usher.Id,
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
                    ApplicationStatus: usher.ApprovalStatus,
                    SubmittedAt: usher.CreatedAt
                ));
        }
    }
}
