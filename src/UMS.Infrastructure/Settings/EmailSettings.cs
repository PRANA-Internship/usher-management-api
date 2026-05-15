using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Infrastructure.Settings
{
    public sealed class EmailSettings
    {
        public const string SectionName = "Email";

        public string Host { get; init; } = string.Empty;
        public int Port { get; init; } = 587;
        public string Username { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public string FromEmail { get; init; } = string.Empty;
        public string FromName { get; init; } = "UMS System";
        public bool UseSsl { get; init; } = true;
        public string FrontendUrl { get; init; } = string.Empty;
    }
}
