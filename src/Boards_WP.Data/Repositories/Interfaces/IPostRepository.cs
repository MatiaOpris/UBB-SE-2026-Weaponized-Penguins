using System;
using System.Collections.Generic;
using System.Text;

using Boards_WP.Data.Models;

namespace Boards_WP.Data.Repositories.Interfaces;

public interface IPostsRepository
{
    public void CreatePost(Post post);
    public void DeletePost(int postID);
    public void IncreaseScore(int postID);
    public void DecreaseScore(int postID);
    public void IncreaseCommentsNumber(int postID);
    public Post GetPostByPostID(int postID);
    public List<Post> GetPostsByCommunityIDs(int[] communityIDs, int offset, int limit);
    public List<Post> GetPostExceptCommunityIDs(int[] communityIDs, int offset, int limit);

    public VoteType GetUserVoteForPost(int userId, int postId);
    public void SetUserVoteForPost(int userId, int postId, VoteType vote);

}
