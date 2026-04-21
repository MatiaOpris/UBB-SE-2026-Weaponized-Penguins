using Boards_WP.Data.Services;
using Boards_WP.Data.Models;

namespace Boards_WP.Tests.Services;

public class NotificationsServiceTests
{
    private readonly NotificationsService _service;

    public NotificationsServiceTests()
    {
        _service = new NotificationsService(null!);
    }

    private static Notification MakeNotification(NotificationType type, string actorName = "alice", string postTitle = "my post") =>
        new Notification
        {
            RelatedPost = new Post { Title = postTitle, Owner = new User() },
            Receiver = new User { Username = "bob" },
            Actor = new User { Username = actorName },
            ActionType = type
        };

    // ── GetNotificationMessage ─────────────────────────────────────────────────

    [Theory]
    [InlineData(NotificationType.CommentOnPost, "alice", "my post", "alice shared their thoughts on your post my post")]
    [InlineData(NotificationType.ReplyToComment, "alice", "my post", "alice replied to your comment on post my post")]
    [InlineData(NotificationType.PostDeleted, "alice", "my post", "Your post 'my post' was deleted")]
    [InlineData(NotificationType.CommentDeleted, "alice", "my post", "Your comment in 'my post' was deleted")]
    public void GetNotificationMessage_KnownType_ReturnsExpectedMessage(
        NotificationType type, string actorName, string postTitle, string expectedMessage)
    {
        // Arrange
        var notification = MakeNotification(type, actorName, postTitle);

        // Act
        var result = _service.GetNotificationMessage(notification);

        // Assert
        Assert.Equal(expectedMessage, result);
    }

    [Fact]
    public void GetNotificationMessage_UnknownType_ReturnsEmptyString()
    {
        // Arrange
        var notification = MakeNotification((NotificationType)99);

        // Act
        var result = _service.GetNotificationMessage(notification);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetNotificationMessage_NullRelatedPost_UsesUnknownPostFallback()
    {
        // Arrange
        var notification = new Notification
        {
            RelatedPost = null!,
            Receiver = new User { Username = "bob" },
            Actor = new User { Username = "alice" },
            ActionType = NotificationType.CommentOnPost
        };

        // Act
        var result = _service.GetNotificationMessage(notification);

        // Assert
        Assert.Contains("unknown post", result);
    }
}
