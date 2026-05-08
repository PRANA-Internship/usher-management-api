using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Domain.Enums
{
    // i put all enums in one file because they are small and related to each other,
    //  but if they grow in the future we can split them into separate files
    public enum UserRole { GUEST, USHER, EVENT_COORDINATOR, ADMIN }
    public enum UserStatus { ACTIVE, INACTIVE, BANNED }
    public enum ApprovalStatus { PENDING, APPROVED, REJECTED }
    public enum TokenType { EmailVerification, PasswordReset }
    public enum Gender { MALE, FEMALE };
}
