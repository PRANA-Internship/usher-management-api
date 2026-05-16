using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Application.Common.Interfaces;
using UMS.Contracts.Auth;
using UMS.Domain.Common;
using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Auth.Commands.RefreshToken
{

    public sealed record RefreshTokenCommand(string RefreshToken)
        : IRequest<Result<RefreshTokenResponse>>;
    public sealed class RefreshTokenCommandHandler(
    IUserRepository userRepository,
    ITokenService tokenService
) : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
    {
        public async Task<Result<RefreshTokenResponse>> Handle(
            RefreshTokenCommand request,
            CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByRefreshTokenAsync(
                request.RefreshToken, cancellationToken);

            if (user is null)
                return AuthErrors.InvalidRefreshToken;

            if (!user.IsRefreshTokenValid(request.RefreshToken))
                return AuthErrors.RefreshTokenExpired;

            // the refresh toke arch is replace the token with new one
            var newAccessToken = tokenService.GenerateAccessToken(user);
            var newRefreshToken = tokenService.GenerateRefreshToken();
            var refreshExpiry = tokenService.GetRefreshTokenExpiry();
            var accessExpiry = tokenService.GetAccessTokenExpiry();

            user.SetRefreshToken(newRefreshToken, refreshExpiry);
            await userRepository.UpdateAsync(user, cancellationToken);

            return Result<RefreshTokenResponse>.Success(new RefreshTokenResponse(
                AccessToken: newAccessToken,
                RefreshToken: newRefreshToken,
                AccessTokenExpiry: accessExpiry
            ));
        }
    }

}
