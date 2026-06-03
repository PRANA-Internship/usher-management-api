using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Domain.Enums
{
    public enum AttendanceStatus
    {
        NotMarked = -1,  // default not used on db
        Absent = 0,
        Late = 1,
        Present = 2
    }
}
