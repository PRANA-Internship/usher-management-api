using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Infrastructure.Cache
{

    public static class CacheKeys
    {
        public static string AllEvents => "prana:events:all";
        public static string EventById(string id) => $"prana:events:{id}";

        public static class TTL
        {
            public static readonly TimeSpan Events = TimeSpan.FromMinutes(5);
        }
    }
}
