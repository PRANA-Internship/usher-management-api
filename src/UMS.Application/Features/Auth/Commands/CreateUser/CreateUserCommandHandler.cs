using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Application.Common.Interfaces;
using UMS.Contracts.Auth;
using UMS.Domain.Common;
using UMS.Domain.Entities;
using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Auth.Commands.CreateUser
{
    public sealed class CreateUserCommandHandler(
   IUserRepository userRepository,
   IPasswordHasher passwordHasher,

    ITokenService tokenService
) : IRequestHandler<CreateUserCommand, Result<AuthResponse>>
    {
        public async Task<Result<AuthResponse>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            if (await userRepository.ExistsByEmailAsync(email, cancellationToken))
                return AuthErrors.EmailAlreadyExists;

            var passwordHash = passwordHasher.Hash(request.Password);
            var createUser = User.CreateUser(request.FullName, email, request.Phone, passwordHash);

            await userRepository.AddAsync(createUser, cancellationToken);

            var accessToken = tokenService.GenerateAccessToken(createUser);
            var refreshToken = tokenService.GenerateRefreshToken();
            var refreshExpiry = tokenService.GetRefreshTokenExpiry();
            var accessExpiry = tokenService.GetAccessTokenExpiry();

            createUser.SetRefreshToken(refreshToken, refreshExpiry);
            await userRepository.UpdateAsync(createUser, cancellationToken);
            return Result<AuthResponse>.Success(new AuthResponse(
               AccessToken: accessToken,
                RefreshToken: refreshToken,
                AccessTokenExpiry: accessExpiry,
                Email: createUser.Email,
                FullName: createUser.FullName,
                Role: createUser.Role.ToString()

                ));
        }
    }
}
