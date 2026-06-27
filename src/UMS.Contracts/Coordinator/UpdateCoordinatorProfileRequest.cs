using System;
using System.Collections.Generic;
using System.Text;

public sealed record UpdateCoordinatorProfileRequest(
    string FullName = "",
    string Phone = ""
);