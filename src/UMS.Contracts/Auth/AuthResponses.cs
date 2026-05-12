using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Contracts.Auth
{

    public sealed record AuthResponse(
        string AccessToken,
        string RefreshToken,
        DateTimeOffset AccessTokenExpiry,
        string Email,
        string FullName,
        string Role
    );
    public sealed record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiry
);
}
