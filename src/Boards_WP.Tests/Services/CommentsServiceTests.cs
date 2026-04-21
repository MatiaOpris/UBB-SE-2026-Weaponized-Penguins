using Moq;
using Boards_WP.Data.Services;
using Boards_WP.Data.Models;
using Boards_WP.Data.Repositories.Interfaces;

namespace Boards_WP.Tests.Services;

public class CommentsServiceTests
{
    private readonly Mock<ICommentsRepository> _commentsRepoMock;
    private readonly Mock<INotificationRepository> _notificationsRepoMock;
    private readonly CommentsService _service;

    public CommentsServiceTests()
    {
        _commentsRepoMock = new Mock<ICommentsRepository>();
        _notificationsRepoMock = new Mock<INotificationRepository>();
        _service = new CommentsService(_commentsRepoMock.Object, _notificationsRepoMock.Object);
    }

    private static Comment MakeValidComment() => new Comment
    {
        Description = "Valid comment text",
        Indentation = 0,
        IsDeleted = false,
        UserCurrentVote = VoteType.None
    };

    // ── ValidateComment ────────────────────────────────────────────────────────

    [Fact]
    public void ValidateComment_EmptyDescription_ThrowsArgumentException()
    {
        // Arrange
        var comment = MakeValidComment();
        comment.Description = "";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => CommentsService.ValidateComment(comment));
    }

    [Fact]
    public void ValidateComment_WhitespaceDescription_ThrowsArgumentException()
    {
        // Arrange
        var comment = MakeValidComment();
        comment.Description = "   ";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => CommentsService.ValidateComment(comment));
    }

    [Fact]
    public void ValidateComment_DescriptionExceedsMaxLength_ThrowsArgumentException()
    {
        // Arrange
        var comment = MakeValidComment();
        comment.Description = new string('a', 619);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => CommentsService.ValidateComment(comment));
    }

    [Fact]
    public void ValidateComment_IndentationExceedsMaxLevel_ThrowsArgumentException()
    {
        // Arrange
        var comment = MakeValidComment();
        comment.Indentation = 8;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => CommentsService.ValidateComment(comment));
    }

    [Fact]
    public void ValidateComment_ValidComment_DoesNotThrow()
    {
        // Arrange
        var comment = MakeValidComment();

        // Act
        var exception = Record.Exception(() => CommentsService.ValidateComment(comment));

        // Assert
        Assert.Null(exception);
    }

    // ── IncreaseScore ──────────────────────────────────────────────────────────

    [Fact]
    public void IncreaseScore_WhenCommentIsDeleted_ThrowsInvalidOperationException()
    {
        // Arrange
        var comment = MakeValidComment();
        comment.IsDeleted = true;

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _service.IncreaseScore(comment, currentUserID: 1));
    }

    [Fact]
    public void IncreaseScore_WhenAlreadyLiked_DecreasesScoreAndSetsVoteToNone()
    {
        // Arrange
        var comment = MakeValidComment();
        comment.UserCurrentVote = VoteType.Like;

        // Act
        _service.IncreaseScore(comment, currentUserID: 1);

        // Assert
        _commentsRepoMock.Verify(r => r.DecreaseScore(comment), Times.Once());
        _commentsRepoMock.Verify(r => r.UpsertUserCommentVote(comment.CommentID, 1, VoteType.None), Times.Once());
        Assert.Equal(VoteType.None, comment.UserCurrentVote);
    }

    [Fact]
    public void IncreaseScore_WhenDisliked_IncreasesScoreTwiceAndSetsVoteToLike()
    {
        // Arrange
        var comment = MakeValidComment();
        comment.UserCurrentVote = VoteType.Dislike;

        // Act
        _service.IncreaseScore(comment, currentUserID: 1);

        // Assert
        _commentsRepoMock.Verify(r => r.IncreaseScore(comment), Times.Exactly(2));
        _commentsRepoMock.Verify(r => r.UpsertUserCommentVote(comment.CommentID, 1, VoteType.Like), Times.Once());
        Assert.Equal(VoteType.Like, comment.UserCurrentVote);
    }

    [Fact]
    public void IncreaseScore_WhenNeutral_IncreasesScoreOnceAndSetsVoteToLike()
    {
        // Arrange
        var comment = MakeValidComment();
        comment.UserCurrentVote = VoteType.None;

        // Act
        _service.IncreaseScore(comment, currentUserID: 1);

        // Assert
        _commentsRepoMock.Verify(r => r.IncreaseScore(comment), Times.Once());
        _commentsRepoMock.Verify(r => r.UpsertUserCommentVote(comment.CommentID, 1, VoteType.Like), Times.Once());
        Assert.Equal(VoteType.Like, comment.UserCurrentVote);
    }

    // ── DecreaseScore ──────────────────────────────────────────────────────────

    [Fact]
    public void DecreaseScore_WhenCommentIsDeleted_ThrowsInvalidOperationException()
    {
        // Arrange
        var comment = MakeValidComment();
        comment.IsDeleted = true;

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _service.DecreaseScore(comment, currentUserID: 1));
    }

    [Fact]
    public void DecreaseScore_WhenAlreadyDisliked_IncreasesScoreAndSetsVoteToNone()
    {
        // Arrange
        var comment = MakeValidComment();
        comment.UserCurrentVote = VoteType.Dislike;

        // Act
        _service.DecreaseScore(comment, currentUserID: 1);

        // Assert
        _commentsRepoMock.Verify(r => r.IncreaseScore(comment), Times.Once());
        _commentsRepoMock.Verify(r => r.UpsertUserCommentVote(comment.CommentID, 1, VoteType.None), Times.Once());
        Assert.Equal(VoteType.None, comment.UserCurrentVote);
    }

    [Fact]
    public void DecreaseScore_WhenLiked_DecreasesScoreTwiceAndSetsVoteToDislike()
    {
        // Arrange
        var comment = MakeValidComment();
        comment.UserCurrentVote = VoteType.Like;

        // Act
        _service.DecreaseScore(comment, currentUserID: 1);

        // Assert
        _commentsRepoMock.Verify(r => r.DecreaseScore(comment), Times.Exactly(2));
        _commentsRepoMock.Verify(r => r.UpsertUserCommentVote(comment.CommentID, 1, VoteType.Dislike), Times.Once());
        Assert.Equal(VoteType.Dislike, comment.UserCurrentVote);
    }

    [Fact]
    public void DecreaseScore_WhenNeutral_DecreasesScoreOnceAndSetsVoteToDislike()
    {
        // Arrange
        var comment = MakeValidComment();
        comment.UserCurrentVote = VoteType.None;

        // Act
        _service.DecreaseScore(comment, currentUserID: 1);

        // Assert
        _commentsRepoMock.Verify(r => r.DecreaseScore(comment), Times.Once());
        _commentsRepoMock.Verify(r => r.UpsertUserCommentVote(comment.CommentID, 1, VoteType.Dislike), Times.Once());
        Assert.Equal(VoteType.Dislike, comment.UserCurrentVote);
    }

    // ── AddComment ─────────────────────────────────────────────────────────────

    [Fact]
    public void AddComment_WhenOwnerDiffersFromActor_AddsNotification()
    {
        // Arrange
        var postOwner = new User { UserID = 1 };
        var commentAuthor = new User { UserID = 2 };
        var post = new Post { Owner = postOwner };
        var comment = MakeValidComment();
        comment.ParentPost = post;
        comment.Owner = commentAuthor;
        comment.ParentComment = null;

        // Act
        _service.AddComment(comment);

        // Assert
        _notificationsRepoMock.Verify(r => r.AddNotification(It.IsAny<Notification>()), Times.Once());
    }

    [Fact]
    public void AddComment_WhenOwnerSameAsActor_DoesNotAddNotification()
    {
        // Arrange
        var user = new User { UserID = 1 };
        var post = new Post { Owner = user };
        var comment = MakeValidComment();
        comment.ParentPost = post;
        comment.Owner = user;
        comment.ParentComment = null;

        // Act
        _service.AddComment(comment);

        // Assert
        _notificationsRepoMock.Verify(r => r.AddNotification(It.IsAny<Notification>()), Times.Never());
    }

    [Fact]
    public void AddComment_WhenNoParentComment_SendsCommentOnPostNotification()
    {
        // Arrange
        var postOwner = new User { UserID = 1 };
        var commentAuthor = new User { UserID = 2 };
        var post = new Post { Owner = postOwner };
        var comment = MakeValidComment();
        comment.ParentPost = post;
        comment.Owner = commentAuthor;
        comment.ParentComment = null;

        // Act
        _service.AddComment(comment);

        // Assert
        _notificationsRepoMock.Verify(r => r.AddNotification(
            It.Is<Notification>(n => n.ActionType == NotificationType.CommentOnPost)), Times.Once());
    }

    [Fact]
    public void AddComment_WhenHasParentComment_SendsReplyToCommentNotification()
    {
        // Arrange
        var postOwner = new User { UserID = 1 };
        var commentAuthor = new User { UserID = 2 };
        var post = new Post { Owner = postOwner };
        var parentComment = new Comment { CommentID = 10, ParentPost = post, Owner = postOwner };
        var comment = MakeValidComment();
        comment.ParentPost = post;
        comment.Owner = commentAuthor;
        comment.ParentComment = parentComment;

        // Act
        _service.AddComment(comment);

        // Assert
        _notificationsRepoMock.Verify(r => r.AddNotification(
            It.Is<Notification>(n => n.ActionType == NotificationType.ReplyToComment)), Times.Once());
    }

    [Fact]
    public void AddComment_WhenParentPostIsNull_DoesNotAddNotification()
    {
        // Arrange
        var comment = MakeValidComment();
        comment.ParentPost = null!;
        comment.Owner = new User { UserID = 1 };

        // Act
        _service.AddComment(comment);

        // Assert
        _notificationsRepoMock.Verify(r => r.AddNotification(It.IsAny<Notification>()), Times.Never());
    }

    [Fact]
    public void AddComment_WhenPostHasNoOwner_DoesNotAddNotification()
    {
        // Arrange
        var commentAuthor = new User { UserID = 1 };
        var post = new Post { Owner = null! };
        var comment = MakeValidComment();
        comment.ParentPost = post;
        comment.Owner = commentAuthor;
        comment.ParentComment = null;

        // Act
        _service.AddComment(comment);

        // Assert
        _notificationsRepoMock.Verify(r => r.AddNotification(It.IsAny<Notification>()), Times.Never());
    }

    // ── GetCommentsByPost ──────────────────────────────────────────────────────

    [Fact]
    public void GetCommentsByPost_WithNoComments_ReturnsEmptyList()
    {
        // Arrange
        _commentsRepoMock.Setup(r => r.GetCommentsByPostID(1, 1)).Returns(new List<Comment>());

        // Act
        var result = _service.GetCommentsByPost(postID: 1, currentUserID: 1);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetCommentsByPost_WithFlatComments_ReturnsHigherScoredFirst()
    {
        // Arrange
        var lowScore = new Comment { CommentID = 1, Score = 1, CreationTime = new DateTime(2020, 1, 1) };
        var highScore = new Comment { CommentID = 2, Score = 100, CreationTime = new DateTime(2020, 1, 1) };
        _commentsRepoMock.Setup(r => r.GetCommentsByPostID(1, 1))
            .Returns(new List<Comment> { lowScore, highScore });

        // Act
        var result = _service.GetCommentsByPost(postID: 1, currentUserID: 1);

        // Assert
        Assert.Equal(highScore.CommentID, result[0].CommentID);
        Assert.Equal(lowScore.CommentID, result[1].CommentID);
    }

    [Fact]
    public void GetCommentsByPost_WithNestedComment_ReturnsChildAfterParent()
    {
        // Arrange
        var parent = new Comment { CommentID = 1, Score = 10, CreationTime = new DateTime(2020, 1, 1) };
        var child = new Comment { CommentID = 2, Score = 100, CreationTime = new DateTime(2020, 1, 1), ParentComment = parent };
        _commentsRepoMock.Setup(r => r.GetCommentsByPostID(1, 1))
            .Returns(new List<Comment> { parent, child });

        // Act
        var result = _service.GetCommentsByPost(postID: 1, currentUserID: 1);

        // Assert
        Assert.Equal(parent.CommentID, result[0].CommentID);
        Assert.Equal(child.CommentID, result[1].CommentID);
    }

    [Fact]
    public void GetCommentsByPost_WithSiblingComments_ReturnsHigherScoredSiblingFirst()
    {
        // Arrange
        var parent = new Comment { CommentID = 1, Score = 50, CreationTime = new DateTime(2020, 1, 1) };
        var weakSibling = new Comment { CommentID = 2, Score = 1, CreationTime = new DateTime(2020, 1, 1), ParentComment = parent };
        var strongSibling = new Comment { CommentID = 3, Score = 100, CreationTime = new DateTime(2020, 1, 1), ParentComment = parent };
        _commentsRepoMock.Setup(r => r.GetCommentsByPostID(1, 1))
            .Returns(new List<Comment> { parent, weakSibling, strongSibling });

        // Act
        var result = _service.GetCommentsByPost(postID: 1, currentUserID: 1);

        // Assert
        Assert.Equal(parent.CommentID, result[0].CommentID);
        Assert.Equal(strongSibling.CommentID, result[1].CommentID);
        Assert.Equal(weakSibling.CommentID, result[2].CommentID);
    }
}
