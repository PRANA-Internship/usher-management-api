using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Domain.Enums
{
    public enum NotificationType
    {

        // Admin notifications
        NewUsherApplication,
        StaffPasswordSet,

        // Coordinator notifications
        ScheduleAssigned,
        UsherAcceptedInvitation,
        UsherAppliedToSchedule,

        // Usher notifications
        InvitedToSchedule,
        ApplicationApproved
    }
}