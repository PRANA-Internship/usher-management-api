using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Application.Common.Interfaces;
using UMS.Contracts.Notification;
using UMS.Domain.Enums;
using UMS.Domain.Entities;
using UMS.Infrastructure.Hubs;

namespace UMS.Infrastructure.Notifications
{

    public sealed class NotificationService(
        IHubContext<NotificationHub> hub,
        INotificationRepository notificationRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork
    ) : INotificationService
    {

        public async Task NotifyAdminsNewUsherApplicationAsync(
            string usherFullName, CancellationToken ct = default)
        {
            var adminIds = await userRepository
                .GetUserIdsByRoleAsync(UserRole.ADMIN, ct);

            await SendToManyAsync(
                recipientIds: adminIds,
                type: NotificationType.NewUsherApplication,
                title: "New Usher Application",
                message: $"{usherFullName} has submitted an application and is pending review.",
                ct: ct);
        }

        public async Task NotifyAdminsStaffPasswordSetAsync(
            string staffFullName, string role, CancellationToken ct = default)
        {
            var adminIds = await userRepository
                .GetUserIdsByRoleAsync(UserRole.ADMIN, ct);

            await SendToManyAsync(
                recipientIds: adminIds,
                type: NotificationType.StaffPasswordSet,
                title: "New Staff Member Joined",
                message: $"{staffFullName} has completed registration as {role}.",
                ct: ct);
        }


        public async Task NotifyCoordinatorScheduleAssignedAsync(
            Guid coordinatorId,
            string venue,
            string startDate,
            CancellationToken ct = default)
        {
            await SendToOneAsync(
                recipientId: coordinatorId,
                type: NotificationType.ScheduleAssigned,
                title: "New Schedule Assignment",
                message: $"You have been assigned to an event venue:{venue} starting {startDate}.",
                ct: ct);
        }

        public async Task NotifyCoordinatorUsherAcceptedAsync(
            Guid coordinatorId,
            string usherFullName,
            CancellationToken ct = default)
        {
            await SendToOneAsync(
                recipientId: coordinatorId,
                type: NotificationType.UsherAcceptedInvitation,
                title: "Invitation Accepted",
                message: $"{usherFullName} has accepted your invitation for you event.",
                ct: ct);
        }

        public async Task NotifyCoordinatorUsherAppliedAsync(
            Guid coordinatorId,
            string usherFullName,
            CancellationToken ct = default)
        {
            await SendToOneAsync(
                recipientId: coordinatorId,
                type: NotificationType.UsherAppliedToSchedule,
                title: "New Application",
                message: $"{usherFullName} has applied to your schedule.",
                ct: ct);
        }

        public async Task NotifyUsherInvitedAsync(
            Guid userId,
            string venue,
            string startDate,
            CancellationToken ct = default)
        {
            await SendToOneAsync(
                recipientId: userId,
                type: NotificationType.InvitedToSchedule,
                title: "You Have Been Invited",
                message: $"You have been invited for an event venue:{venue} starting :{startDate}  see more about the event on your portal.",
                ct: ct);
        }

        public async Task NotifyUsherApplicationApprovedAsync(
            Guid userId,
            CancellationToken ct = default)
        {
            await SendToOneAsync(
                recipientId: userId,
                type: NotificationType.ApplicationApproved,
                title: "Application Approved",
                message: $"Your application has been approved see more on your portal.",
                ct: ct);
        }


        private async Task SendToOneAsync(
            Guid recipientId,
            NotificationType type,
            string title,
            string message,
            string? payload = null,
            CancellationToken ct = default)
        {
            var notification = Notification.Create(
                recipientId, type, title, message, payload);

            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await notificationRepository.AddAsync(notification, ct);
            }, ct);

            var dto = ToDto(notification);

            await hub.Clients
                .Group(recipientId.ToString())
                .SendAsync("ReceiveNotification", dto, ct);
        }

        private async Task SendToManyAsync(
            IReadOnlyList<Guid> recipientIds,
            NotificationType type,
            string title,
            string message,
            string? payload = null,
            CancellationToken ct = default)
        {
            if (!recipientIds.Any()) return;

            var notifications = recipientIds
                .Select(id => Notification.Create(id, type, title, message, payload))
                .ToList();

            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await notificationRepository.AddRangeAsync(notifications, ct);
            }, ct);

            foreach (var notification in notifications)
            {
                await hub.Clients
                    .Group(notification.RecipientId.ToString())
                    .SendAsync("ReceiveNotification", ToDto(notification), ct);
            }
        }

        private static NotificationDto ToDto(Notification n) =>
            new(
                Id: n.Id,
                Type: n.Type.ToString(),
                Title: n.Title,
                Message: n.Message,
                IsRead: n.IsRead,
                CreatedAt: n.CreatedAt,
                Payload: n.Payload
            );
    }

}
