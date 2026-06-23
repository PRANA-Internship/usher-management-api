using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Contracts.Usher
{

    public sealed record RespondToInvitationRequest(Guid UsherId, bool Accept, Guid invitationId);
}