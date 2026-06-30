namespace UMS.Infrastructure.Settings
{
    public sealed class RateLimiterSettings
    {
        public const string SectionName = "RateLimiter";

        public int PermitLimit { get; init; } = 20;
        public int WindowMinutes { get; init; } = 5;
        public int QueueLimit { get; init; } = 0;
        public string PolicyName { get; init; } = "fixed";
    }
}