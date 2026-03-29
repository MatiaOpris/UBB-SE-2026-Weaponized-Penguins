using System;
using System.Collections.Generic;
using System.Text;

using Boards_WP.Data.Models;
using Boards_WP.Data.Repositories.Interfaces;
using Boards_WP.Data.Services.Interfaces;

namespace Boards_WP.Data.Services;


internal class CommentsService : ICommentsService
{
    private readonly ICommentsRepository commentsRepo;
    //private readonly INotificationsRepository notificationsRepo;
    const int MAX_DESCRIPTION_LENGTH = 618;
    const int MAX_INDENTATION_LEVEL = 7;

    public CommentsService(ICommentsRepository commentsReop/*, INotificationsRepository notificationsRepo*/)
    {
        this.commentsRepo = commentsReop;
        //this.notificationsRepo = notificationsRepo;
    }

    public void AddComment(Comment c)
    {
        ValidateComment(c);
        c.CreationTime = DateTime.Now;
        c.IsDeleted = false;
        commentsRepo.AddComment(c);
        //notificationsRepo.addNotification(c);
    }

    public void SoftDeleteComment(Comment c, int userID)
    {
        c.Description = "[deleted]";
        c.IsDeleted= true;
        commentsRepo.SoftDeleteComment(c.CommentID);
    }
    public void IncreaseComment(Comment c, int currentUserID)
    {
        if (c.IsDeleted)
            throw new InvalidOperationException("Cannot vote on a deleted comment.");

        if (c.UserCurrentVote == VoteType.Like)
        {
            commentsRepo.DecreaseScore(c);
            commentsRepo.UpsertUserCommentVote(c.CommentID, currentUserID, VoteType.None);
            c.UserCurrentVote = VoteType.None;
        }
        else if (c.UserCurrentVote == VoteType.Dislike)
        {
            commentsRepo.IncreaseScore(c);
            commentsRepo.IncreaseScore(c);
            commentsRepo.UpsertUserCommentVote(c.CommentID, currentUserID, VoteType.Like);
            c.UserCurrentVote = VoteType.Like;
        }
        else
        {
            commentsRepo.IncreaseScore(c);
            commentsRepo.UpsertUserCommentVote(c.CommentID, currentUserID, VoteType.Like);
            c.UserCurrentVote = VoteType.Like;
        }
    }
    public void DecreaseComment(Comment c, int currentUserID)
    {
        if (c.IsDeleted)
            throw new InvalidOperationException("Cannot vote on a deleted comment.");

        if (c.UserCurrentVote == VoteType.Dislike)
        {
            commentsRepo.IncreaseScore(c);
            commentsRepo.UpsertUserCommentVote(c.CommentID, currentUserID, VoteType.None);
            c.UserCurrentVote = VoteType.None;
        }
        else if (c.UserCurrentVote == VoteType.Like)
        {
            commentsRepo.DecreaseScore(c);
            commentsRepo.DecreaseScore(c);
            commentsRepo.UpsertUserCommentVote(c.CommentID, currentUserID, VoteType.Dislike);
            c.UserCurrentVote = VoteType.Dislike;
        }
        else
        {
            commentsRepo.DecreaseScore(c);
            commentsRepo.UpsertUserCommentVote(c.CommentID, currentUserID, VoteType.Dislike);
            c.UserCurrentVote = VoteType.Dislike;
        }

    }
    public List<Comment> GetCommentsByPost(int postID, int currentUserID)
    {
        return commentsRepo.GetCommentsByPostID(postID, currentUserID);
    }
    public static void ValidateComment(Comment c)
    {
        if (string.IsNullOrWhiteSpace(c.Description))
            throw new ArgumentException("Comment description cannot be empty.");
        if (c.Description.Length > MAX_DESCRIPTION_LENGTH)
            throw new ArgumentException("Comment description cannot exceed 1000 characters.");
        if (c.Indentation> MAX_INDENTATION_LEVEL)
            throw new ArgumentException("Comment indentation cannot exceed 7 levels.");
    }

    public List<Comment> getCommentsByPost(int postID, int currentUserID)
    {
        return commentsRepo.GetCommentsByPostID(postID, currentUserID);
    }
}

