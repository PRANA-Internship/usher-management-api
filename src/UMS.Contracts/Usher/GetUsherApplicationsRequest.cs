
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Domain.Enums;

namespace UMS.Contracts.Usher
{
    //this is for pagination purpose
    public sealed class GetUsherApplicationsRequest
    {
        public int Page { get; set; } = 1;
        public int Size { get; set; } = 10;
        public ApprovalStatus? Status { get; set; }
    }
}
