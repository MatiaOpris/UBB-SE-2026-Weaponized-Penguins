using System;
using System.Collections.Generic;
using System.Text;

using Boards_WP.Data.Models;

namespace Boards_WP.Data.Repositories.Interfaces;

public interface ICommentsRepository
{
    void AddComment(Comment c);
    void SoftDeleteComment(int commentID);
    void IncreaseScore(Comment c);
    void DecreaseScore(Comment c);
    List<Comment> GetCommentsByPostID(int postID, int userID);
    void UpsertUserCommentVote(int commentID, int currentUserID, VoteType vote);
}
