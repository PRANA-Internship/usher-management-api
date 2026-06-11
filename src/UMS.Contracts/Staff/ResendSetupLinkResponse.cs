using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Contracts.Staff
{
    public sealed record ResendSetupLinkResponse(
     Guid UserId,
     string Email,
     string Message
 );
}