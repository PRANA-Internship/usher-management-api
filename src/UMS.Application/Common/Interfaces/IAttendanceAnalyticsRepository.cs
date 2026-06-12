using System;
using System.Collections.Generic;
using System.Text;

using UMS.Contracts.Admin.Dashboard;
namespace UMS.Application.Common.Interfaces
{
    public interface IAttendanceAnalyticsRepository
    {
        Task<IReadOnlyList<MonthlyAttendanceData>> GetMonthlyTrendAsync(
            int months, CancellationToken ct = default);
    }
}