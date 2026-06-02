using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Contracts.Coordinator
{
    public sealed record RespondToApplicationRequest(bool Accept, Guid ApplicationId);
}
