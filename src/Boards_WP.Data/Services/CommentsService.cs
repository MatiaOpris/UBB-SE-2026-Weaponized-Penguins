using System;
using System.Collections.Generic;
using System.Linq;
using Boards_WP.Data.Models;
using Boards_WP.Data.Repositories.Interfaces;
using Boards_WP.Data.Services.Interfaces;

namespace Boards_WP.Data.Services;


public class CommentsService : ICommentsService
{
    private readonly ICommentsRepository commentsRepo;
    private readonly INotificationRepository notificationsRepo;
    const int MAX_DESCRIPTION_LENGTH = 618;
    const int MAX_INDENTATION_LEVEL = 7;

    public CommentsService(ICommentsRepository commentsRepo, INotificationRepository notificationsRepo)
    {
        this.commentsRepo = commentsRepo;
        this.notificationsRepo = notificationsRepo;
    }

    public void AddComment(Comment comment)
    {
        ValidateComment(comment);
        comment.CreationTime = DateTime.Now;
        comment.IsDeleted = false;
        commentsRepo.AddComment(comment);

        if (comment.ParentPost != null && comment.Owner != null)
        {
            var notification = new Notification
            {
                RelatedPost = comment.ParentPost,
                Receiver = comment.ParentPost.Owner,
                Actor = comment.Owner,
                ActionType = comment.ParentComment == null ? NotificationType.CommentOnPost : NotificationType.ReplyToComment
            };
            if (notification.Receiver != null && notification.Receiver.UserID != notification.Actor.UserID)
            {
                notificationsRepo.AddNotification(notification);
            }
        }
    }

    public void SoftDeleteComment(Comment comment, int userID)
    {
        comment.Description = "[deleted]";
        comment.IsDeleted= true;
        commentsRepo.SoftDeleteComment(comment.CommentID);
    }
    public void IncreaseScore(Comment comment, int currentUserID)
    {
        if (comment.IsDeleted)
            throw new InvalidOperationException("Cannot vote on a deleted comment.");

        if (comment.UserCurrentVote == VoteType.Like)
        {
            commentsRepo.DecrementCommentScore(comment);
            commentsRepo.UpsertUserCommentVote(comment.CommentID, currentUserID, VoteType.None);
            comment.UserCurrentVote = VoteType.None;
        }
        else if (comment.UserCurrentVote == VoteType.Dislike)
        {
            commentsRepo.IncreaseScore(comment);
            commentsRepo.IncreaseScore(comment);
            commentsRepo.UpsertUserCommentVote(comment.CommentID, currentUserID, VoteType.Like);
            comment.UserCurrentVote = VoteType.Like;
        }
        else
        {
            commentsRepo.IncreaseScore(comment);
            commentsRepo.UpsertUserCommentVote(comment.CommentID, currentUserID, VoteType.Like);
            comment.UserCurrentVote = VoteType.Like;
        }
    }
    public void DecreaseScore(Comment comment, int currentUserID)
    {
        if (comment.IsDeleted)
            throw new InvalidOperationException("Cannot vote on a deleted comment.");

        if (comment.UserCurrentVote == VoteType.Dislike)
        {
            commentsRepo.IncreaseScore(comment);
            commentsRepo.UpsertUserCommentVote(comment.CommentID, currentUserID, VoteType.None);
            comment.UserCurrentVote = VoteType.None;
        }
        else if (comment.UserCurrentVote == VoteType.Like)
        {
            commentsRepo.DecrementCommentScore(comment);
            commentsRepo.DecrementCommentScore(comment);
            commentsRepo.UpsertUserCommentVote(comment.CommentID, currentUserID, VoteType.Dislike);
            comment.UserCurrentVote = VoteType.Dislike;
        }
        else
        {
            commentsRepo.DecrementCommentScore(comment);
            commentsRepo.UpsertUserCommentVote(comment.CommentID, currentUserID, VoteType.Dislike);
            comment.UserCurrentVote = VoteType.Dislike;
        }

    }
    private const int RootParentSentinel = -1;

    public List<Comment> GetCommentsByPost(int postID, int currentUserID)
    {
        var comments = commentsRepo.GetCommentsByPostID(postID, currentUserID);

        var childrenMap = new Dictionary<int, List<Comment>>();
        foreach (var comment in comments)
        {
            int parentId = comment.ParentComment?.CommentID ?? RootParentSentinel;
            if (!childrenMap.ContainsKey(parentId))
                childrenMap[parentId] = new List<Comment>();

            childrenMap[parentId].Add(comment);
        }

        var sortedComments = new List<Comment>();

        void AddSortedChildren(int parentId)
        {
            if (childrenMap.TryGetValue(parentId, out var children))
            {
                var sorted = children.OrderByDescending(c => CalculateBestScore(c)).ToList();
                foreach (var child in sorted)
                {
                    sortedComments.Add(child);
                    AddSortedChildren(child.CommentID);
                }
            }
        }

        AddSortedChildren(RootParentSentinel);

        return sortedComments;
    }

    private double CalculateBestScore(Comment comment)
    {

        double order = Math.Log10(Math.Max(Math.Abs(comment.Score), 1));

        int sign = 0;
        if (comment.Score > 0) sign = 1;
        else if (comment.Score < 0) sign = -1;


        double seconds = (comment.CreationTime - new DateTime(2020, 1, 1)).TotalSeconds;

        return (sign * order) + (seconds / 45000.0);
    }
    public static void ValidateComment(Comment c)
    {
        if (string.IsNullOrWhiteSpace(c.Description))
            throw new ArgumentException("Comment description cannot be empty.");
        if (c.Description.Length > MAX_DESCRIPTION_LENGTH)
            throw new ArgumentException("Comment description cannot exceed 618 characters.");
        if (c.Indentation> MAX_INDENTATION_LEVEL)
            throw new ArgumentException("Comment indentation cannot exceed 7 levels.");
    }
}

