using System;
using System.Collections.Generic;
using System.Text;

using Boards_WP.Data.Models;
using Boards_WP.Data.Repositories.Interfaces;

namespace Boards_WP.Data.Repositories;

public class CommentsRepository : ICommentsRepository
{
    private String _commentsDbContext;
    private String _commentsVotesDbContext;
    private String _usersDbContext;

    public CommentsRepository(String commentsDbContext, String commentsVotesDbContext, String usersDbContext)
    {
        this._commentsDbContext = commentsDbContext;
        this._commentsVotesDbContext = commentsVotesDbContext;
        this._usersDbContext = usersDbContext;
    }

    public void AddComment(Comment c)
    {
        // TODO
    }

    public void SoftDeleteComment(int commentID)
    {
        // TODO
    }
    public void IncreaseScore(Comment c)
    {
        // TODO
    }
    public void DecreaseScore(Comment c)
    {
        // TODO
    }

    public List<Comment> GetCommentsByPostID(int postID, int userID)
    {
        // TODO
        return new List<Comment>();
    }
    public void UpsertUserCommentVote(int commentID, int currentUserID, VoteType vote)
    {
        // TODO
    }

}
