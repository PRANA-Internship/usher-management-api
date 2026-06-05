using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Application.Common.Interfaces;
using UMS.Contracts.Admin.Dashboard;
using UMS.Domain.Common;
using UMS.Infrastructure.Cache;

namespace UMS.Application.Features.Admin.Queries.Dashboard
{

    public sealed record AttendanceTrendQuery : IRequest<Result<AttendanceTrendResponse>>;

    public sealed class GetAttendanceTrendQueryHandler(
        IAttendanceAnalyticsRepository attendanceRepository,
        ICacheService cache
    ) : IRequestHandler<AttendanceTrendQuery, Result<AttendanceTrendResponse>>
    {
        private const int Months = 11; //11 months in the ui

        public async Task<Result<AttendanceTrendResponse>> Handle(
            AttendanceTrendQuery query,
            CancellationToken cancellationToken)
        {
            var cached = await cache.GetAsync<AttendanceTrendResponse>(
                CacheKeys.AdminAttendanceTrend, cancellationToken);

            if (cached is not null)
                return Result<AttendanceTrendResponse>.Success(cached);

            var rawData = await attendanceRepository
                .GetMonthlyTrendAsync(Months, cancellationToken);

            var trend = new List<AttendanceTrendPoint>();
            var now = DateTime.UtcNow;

            for (var i = Months - 1; i >= 0; i--)
            {
                var date = now.AddMonths(-i);
                var year = date.Year;
                var month = date.Month;

                var dataPoint = rawData.FirstOrDefault(
                    d => d.Year == year && d.Month == month);

                double avgPercentage = 0;

                if (dataPoint is not null && dataPoint.MaxPossibleScore > 0)
                {
                    avgPercentage = Math.Round(
                        (double)dataPoint.TotalScore /
                        dataPoint.MaxPossibleScore * 100, 2);
                }

                trend.Add(new AttendanceTrendPoint(
                    Year: year,
                    Month: month,
                    MonthLabel: date.ToString("MMM yyyy"),
                    AveragePercentage: avgPercentage,
                    TotalSessions: dataPoint?.TotalSessions ?? 0,
                    TotalMarked: dataPoint?.TotalMarked ?? 0
                ));
            }

            var pointsWithData = trend.Where(t => t.TotalSessions > 0).ToList();
            var overallAverage = pointsWithData.Count > 0
                ? Math.Round(pointsWithData.Average(t => t.AveragePercentage), 2)
                : 0;

            var response = new AttendanceTrendResponse(
                Trend: trend,
                OverallAverage: overallAverage
            );

            await cache.SetAsync(
                CacheKeys.AdminAttendanceTrend, response, CacheKeys.TTL.AdminDashboardDuration, cancellationToken);

            return Result<AttendanceTrendResponse>.Success(response);
        }
    }

}
