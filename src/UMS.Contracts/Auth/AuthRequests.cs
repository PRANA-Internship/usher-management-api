using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Contracts.Auth
{

    public sealed record LoginRequest(string Email, string Password);
    public sealed record RefreshTokenRequest(string RefreshToken);



    //this is for devlopment only to help us to create admin for testing purpose
    //letter will be removed
    public sealed record CreateUserRequest(
        string FullName,
        string Email,
        string Phone,
        string Password
    );
}

