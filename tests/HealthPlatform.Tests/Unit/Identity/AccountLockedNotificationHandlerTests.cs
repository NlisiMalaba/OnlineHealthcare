using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Identity.EventHandlers;
using HealthPlatform.Application.Identity.Notifications;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.Identity;

public sealed class AccountLockedNotificationHandlerTests
{
    [Fact]
    public async Task Handle_NotifiesAccountOwner()
    {
        var userId = Guid.CreateVersion7();
        var lockoutEnd = DateTimeOffset.UtcNow.AddMinutes(15);
        var notifier = new Mock<IAccountLockoutNotifier>();
        var logger = new Mock<ILogger<AccountLockedNotificationHandler>>();
        var handler = new AccountLockedNotificationHandler(notifier.Object, logger.Object);
        var notification = new AccountLockedNotification(userId, lockoutEnd, 5);

        await handler.Handle(notification, CancellationToken.None);

        notifier.Verify(
            n => n.NotifyAccountLockedAsync(userId, lockoutEnd, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
