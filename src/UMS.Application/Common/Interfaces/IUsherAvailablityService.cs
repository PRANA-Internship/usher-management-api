namespace UMS.Application.Common.Interfaces
{
    public interface IUsherAvailablityService
    {
        Task<bool> IsAvailableAsync(
            Guid usherId,
            DateOnly startDate,
            DateOnly endDate,
            CancellationToken ct = default);
    }
}