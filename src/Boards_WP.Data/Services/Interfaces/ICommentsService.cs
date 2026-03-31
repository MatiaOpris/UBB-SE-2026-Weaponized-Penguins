using System;
using System.Collections.Generic;
using System.Text;

using Boards_WP.Data.Models;

namespace Boards_WP.Data.Services.Interfaces;

public interface ICommentsService
{
    public void AddComment(Comment c);
    public void SoftDeleteComment(Comment c, int userID);
    public void IncreaseScore(Comment c, int currentUserID);
    public void DecreaseScore(Comment c, int currentUserID);
    public List<Comment> GetCommentsByPost(int postID, int currentUserID);

}
