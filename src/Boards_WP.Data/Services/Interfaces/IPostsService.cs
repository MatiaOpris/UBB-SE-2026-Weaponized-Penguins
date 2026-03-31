using System;
using System.Collections.Generic;
using System.Text;

namespace Boards_WP.Data.Services.Interfaces;

public interface IPostsService
{
    public List<Post> GetPostsForHomePage(int userId);
    public List<Post> GetPostsForDiscoveryPage(int userId);
    public void AddPost(Post post);
    public void DeletePost(int postId);
    public void IncreaseScore(int postId);
    public void DecreaseScore(int postId);
    public Post GetPostByPostID(int postId);
    public List<Post> GetPostsByCommunityID(int communityId);
    public ThemeColor DetermineFeedThemeColorByLastLikes();
    public ThemeColor DetermineThemeForASinglePost(Post post);
    public void UpdateUserPreferences(int userID, Post p, bool hasCommented);
    public void UpdateUserInterests(int userId, Post post, VoteType vote, bool hasCommented);
}
