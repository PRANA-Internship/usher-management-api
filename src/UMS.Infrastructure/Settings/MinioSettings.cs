using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Infrastructure.Settings
{
    public sealed class MinioSettings
    {
        public const string SectionName = "Minio";

        public string Endpoint { get; init; } = string.Empty;
        public string AccessKey { get; init; } = string.Empty;
        public string SecretKey { get; init; } = string.Empty;
        public string BucketName { get; init; } = "ums-files";
        public bool UseSSL { get; init; } = false;
        public string PublicBaseUrl { get; init; } = string.Empty;
    }
}
