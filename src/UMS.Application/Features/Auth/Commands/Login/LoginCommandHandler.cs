using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Application.Common.Interfaces;
using UMS.Contracts.Auth;
using UMS.Domain.Common;
using static UMS.Domain.Common.Error;
namespace UMS.Application.Features.Auth.Commands.Login
{
    public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService

        ): IRequestHandler<LoginCommand, Result<AuthResponse>>
    {
     public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
          var email = request.Email.Trim().ToLowerInvariant();

            var user = await userRepository.GetByEmailAsync(email, cancellationToken);
            var hash = user?.PasswordHash ?? "$2a$12$invalidhashpaddingtomakeconstanttime";

            if (user is null || !passwordHasher.Verify(request.Password, hash))
            {
                return AuthErrors.InvalidCredentials;
            }


            var accessToken = tokenService.GenerateAccessToken(user);
            var refreshToken = tokenService.GenerateRefreshToken();
            var refreshExpiry = tokenService.GetRefreshTokenExpiry();
            var accessExpiry = tokenService.GetAccessTokenExpiry();

        //    user.SetRefreshToken(refreshToken, refreshExpiry);
            await userRepository.UpdateAsync(user, cancellationToken);

            return Result<AuthResponse>.Success(new AuthResponse(
                AccessToken: accessToken,
                RefreshToken: refreshToken,
                AccessTokenExpiry: accessExpiry,
                Email: user.Email,
                FullName: user.FullName,
                Role: user.Role.ToString()
            ));
        

    }

    }
}
