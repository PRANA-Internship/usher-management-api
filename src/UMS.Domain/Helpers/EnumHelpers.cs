using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Domain.Helpers
{
    public static class EnumHelpers
    {
        public static string SerializeEnum<T>(IList<T> values) where T : Enum =>
       string.Join(",", values.Select(v => v.ToString()));

        public static IReadOnlyList<T> ParseEnum<T>(string value) where T : struct, Enum =>
            string.IsNullOrWhiteSpace(value)
                ? []
                : value.Split(',')
                       .Select(s => Enum.Parse<T>(s.Trim()))
                       .ToList();
    }
}
