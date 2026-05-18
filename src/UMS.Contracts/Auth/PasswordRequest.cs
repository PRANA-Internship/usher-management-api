using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Contracts.Auth
{
    public sealed record ForgotPasswordRequest(string Email);
    public sealed record ResetPasswordRequest(
      string Token,
      string Password,
      string ConfirmPassword
    );


}
