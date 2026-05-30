using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Domain.Common;

namespace UMS.Application.Features.Ushers.Command.RespondToInvitaion
{
    public sealed record RespondToInvitationCommand(
        Guid InvitationId,
        Guid UsherId,
        bool Accept
    ) : IRequest<Result<bool>>;
}
