using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Infrastructure.Settings
{
    public sealed class RedisSettings
    {
        public const string SectionName = "Redis";
        public string ConnectionString { get; init; } = string.Empty;
        public string InstanceName { get; init; } = "UMS:";
    }
}