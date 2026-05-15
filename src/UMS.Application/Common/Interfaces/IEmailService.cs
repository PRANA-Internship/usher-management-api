using System;
using System.Collections.Generic;
using System.Text;
using UMS.Domain.Entities;

namespace UMS.Application.Common.Interfaces
{
    public interface IEmailService
    {
        Task SendApplicationReceivedAsync(string toEmail, string fullName, CancellationToken ct = default);
        Task SendPasswordSetupAsync(string toEmail, string fullName, string token, CancellationToken ct = default);
    }


}
