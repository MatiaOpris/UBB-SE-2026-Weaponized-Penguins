using System;
using System.Collections.Generic;
using System.Text;

namespace Boards_WP.Data.Services.Interfaces;

public interface IPostsService
{
    public List<Post> GetPostsForHomePage(int userId, int offset, int limit);
    public List<Post> GetPostsForDiscoveryPage(int userId, int offset, int limit);
    public void AddPost(Post post);
    public void DeletePost(int postId);
    public void IncreaseCommentsNumber(int postId);
    public void IncreaseScore(int postId);
    public void DecreaseScore(int postId);
    public Post GetPostByPostID(int postId);
    public List<Post> GetPostsByCommunityIDs(int[] communityIds, int offset, int limit);
    public ThemeColor DetermineFeedThemeColorByLastLikes();
    public ThemeColor DetermineThemeForASinglePost(Post post);
    public ThemeColor CalculateDominantColor(IEnumerable<Post> posts);
    public void UpdateUserInterests(int userId, Post post, VoteType vote, bool hasCommented);
}
