using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Infrastructure.Settings
{
    public sealed class PranaApiSettings
    {
        public const string SectionName = "PranaApi";
        public string BaseUrl { get; init; } = string.Empty;
        public string ApiKey { get; init; } = string.Empty;
    }
}
