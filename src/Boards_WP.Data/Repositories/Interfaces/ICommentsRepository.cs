using System;
using System.Collections.Generic;
using System.Text;

using Boards_WP.Data.Models;

namespace Boards_WP.Data.Repositories.Interfaces;

public interface ICommentsRepository
{
    void AddComment(Comment comment);
    void SoftDeleteComment(int commentID);
    void IncreaseScore(Comment comment);
    void DecrementCommentScore(Comment comment);
    List<Comment> GetCommentsByPostID(int postID, int userID);
    void UpsertUserCommentVote(int commentID, int currentUserID, VoteType vote);
}
