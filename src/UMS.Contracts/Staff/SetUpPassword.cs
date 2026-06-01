using System;
using System.Collections.Generic;
using System.Text;
using UMS.Domain.Common;

namespace UMS.Contracts.Staff
{


    public sealed record SetupPasswordRequest(
        string Token,
        string Password,
        string ConfirmPassword
    );
}