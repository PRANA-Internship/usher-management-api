using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Infrastructure.Cache
{

    public static class CacheKeys
    {
        public static string AllEvents => "prana:events:all";
        public static string AdminDashboard => "admin:dashboard";
        public static string AdminAttendanceTrend => "admin:attendance:trend";

        public static string EventById(string id) => $"prana:events:{id}";
        public static string PaginatedEvents(int pageNumber, int pageSize) => $"prana:events:page:{pageNumber}:size:{pageSize}";
        public static string UsherAnalytics(Guid usherId) => $"usher:analytics:{usherId}";


        public static class TTL
        {
            public static readonly TimeSpan Events = TimeSpan.FromHours(12);

            public static readonly TimeSpan AdminDashboardDuration = TimeSpan.FromHours(1);

            private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);
            public static readonly TimeSpan UsherAnalytics = TimeSpan.FromMinutes(15);

        }
    }
}