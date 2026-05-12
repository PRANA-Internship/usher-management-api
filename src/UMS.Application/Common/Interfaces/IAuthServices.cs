using System;
using System.Collections.Generic;
using System.Text;
using UMS.Domain.Entities;

namespace UMS.Application.Common.Interfaces
{
    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string password, string hash);
    }
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        DateTimeOffset GetAccessTokenExpiry();
        DateTimeOffset GetRefreshTokenExpiry();
    }
}
