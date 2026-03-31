using System;
using System.Collections.Generic;
using System.Text;

namespace Boards_WP.Data.Services;

public class PostsService : IPostsService
{
    private readonly IPostsRepository _postsRepo;
    private readonly IUsersRepository _usersRepo;
    private readonly ITagsRepository _tagsRepo;
    //community service
    private readonly UserSession _userSession;
    private readonly IUsersMoodRepository _usersMoodRepository;
    private int _cachedCategoryCount = 0;
    private readonly int _goldenPostsPercent = 25;

    private List<Post> _lastLikesOfCurrentUser= new List<Post>();  

    public PostsService(IPostsRepository postsRepo, IUsersRepository usersRepo, ITagsRepository _tagsRepo, UserSession userSession, IUsersMoodRepository usersMoodRepository)
    {
        _postsRepo = postsRepo;
        _usersRepo = usersRepo;
        _tagsRepo = _tagsRepo;
        _userSession = userSession;
        _usersMoodRepository = usersMoodRepository;
    }


    public void AddPost(Post post)
    {
        //todo : validate post
        if (string.IsNullOrWhiteSpace(post.Title))
            throw new ArgumentException("Post title cannot be empty.");

        _postsRepo.AddPost(post);
    }


    public Post GetPostByPostID(int postId)
    {
        return _postsRepo.GetPostByPostID(postId);
    }

    public void DeletePost(int postId)
    {
        _postsRepo.DeletePost(postId);
    }

    public void IncreaseScore(int postId)
    {
        
        int userId = _userSession.CurrentUser.UserID;
        VoteType currentVote = _postsRepo.GetUserVoteForPost(userId, postId);

        if (currentVote == VoteType.Like)
        {
            
            _postsRepo.SetUserVoteForPost(userId, postId, VoteType.None);
            _postsRepo.DecreaseScore(postId);
        }
        else if (currentVote == VoteType.Dislike)
        {
            
            _postsRepo.SetUserVoteForPost(userId, postId, VoteType.Like);

            _postsRepo.IncreaseScore(postId);
            _postsRepo.IncreaseScore(postId);
        }
        else
        { 
            _postsRepo.SetUserVoteForPost(userId, postId, VoteType.Like);
            _postsRepo.IncreaseScore(postId);
        }
    }

    public void DecreaseScore(int postId)
    {
        int userId = _userSession.CurrentUser.UserID;
        VoteType currentVote = _postsRepo.GetUserVoteForPost(userId, postId);

        if (currentVote == VoteType.Dislike)
        {
            
            _postsRepo.SetUserVoteForPost(userId, postId, VoteType.None);

            _postsRepo.IncreaseScore(postId);
        }
        else if (currentVote == VoteType.Like)
        {

            _postsRepo.SetUserVoteForPost(userId, postId, VoteType.Dislike);

            _postsRepo.DecreaseScore(postId);
            _postsRepo.DecreaseScore(postId);
        }
        else
        {
            _postsRepo.SetUserVoteForPost(userId, postId, VoteType.Dislike);
            _postsRepo.DecreaseScore(postId);
        }
    }

    public List<Post> GetPostsByCommunityID(int communityId)
    {
        return _postsRepo.GetPostsByCommunityIDs(new[] { communityId });
    }

    public List<Post> GetPostsForHomePage(int userId)
    {
        List<Community> communities = new List<Community>();//= _communitiesRepo.GetCommunitiesUserIsPartOf(userId);

        int[] communityIds = communities.Select(c => c.CommunityID).ToArray();

        return _postsRepo.GetPostsByCommunityIDs(communityIds);
    }

    public List<Post> GetPostsForDiscoveryPage(int userId)
    {
        List<Community> communities = new List<Community>();//= _communitiesRepo.GetCommunitiesUserIsPartOf(userId);

        if(_cachedCategoryCount == 0)
            _cachedCategoryCount = _tagsRepo.GetCategoryCount();
        var userScores = _usersMoodRepository.GetUsersMoodScores(userId, _cachedCategoryCount);
        List<int> allCategoryIds = _tagsRepo.GetAllCategories().Select(c => c.CategoryID).ToList();
        int[] communityIds = communities.Select(c => c.CommunityID).ToArray();
        //--TODO: should get 1k from here and keep only 25% of it
        var candidates = _postsRepo.GetPostExceptCommunityIDs(communityIds);
        var keptPostsCount = candidates.Count() * _goldenPostsPercent / 100;

        var scoredCandidates = candidates.Select(post => new
        {
            OriginalPost = post,
            Score = CalculateManhattanDistance(userScores, post, allCategoryIds)
        })
        .OrderBy(temp => temp.Score)
        .ThenByDescending(temp => temp.OriginalPost.CreationTime)
        .Take(keptPostsCount)
        .Select(temp => temp.OriginalPost)
        .ToList();

        return scoredCandidates;
    }

    public ThemeColor DetermineFeedThemeColorByLastLikes()
    {
        throw new NotImplementedException();
        //todo
    }

    public ThemeColor DetermineThemeForASinglePost(Post post)
    {
        throw new NotImplementedException();
        //todo
    }


    public void UpdateUserPreferences(int userID, Post p, bool hasCommented)
    {
        throw new NotImplementedException();
    }

    private double GetInteractionIntensity(VoteType vote, bool hasCommented)
    {
        double multiplier = hasCommented ? 2.0 : 1.0;

        double baseWeight = vote switch
        {
            VoteType.Like => 1.0,
            VoteType.Dislike => -1.0,
            VoteType.None => 0.5,
            _ => 0.0
        };

        return baseWeight * multiplier;
    }

    public void UpdateUserInterests(int userId, Post post, VoteType vote, bool hasCommented)
    {
        //--all repo/service calls here
        if (_cachedCategoryCount == 0)
            _cachedCategoryCount = _tagsRepo.GetCategoryCount();
        var userScores = _usersMoodRepository.GetUsersMoodScores(userId, _cachedCategoryCount);
        userScores = UserInterestsAlgorithm(userId, post, vote, hasCommented, userScores);
        _usersMoodRepository.UpdateUsersMoodScores(userId, userScores);

    }

    internal Dictionary<int, int> UserInterestsAlgorithm(int userId, Post post, VoteType vote, bool hasCommented, Dictionary<int, int> userScores)
    {
        //--all testable math here
        double intensity = GetInteractionIntensity(vote, hasCommented);
        if (intensity == 0) return userScores;

        int appliedChange = 0;

        for (int i = 0; i < post.Tags.Count; i++)
        {
            int catId = post.Tags[i].CategoryBelongingTo.CategoryID;
            int change = (int)Math.Round(((10 - i) * 10) * intensity);
            userScores[catId] += change;
            appliedChange += change;
        }

        var others = userScores.Keys.Where(id => !post.Tags.Any(t => t.CategoryBelongingTo.CategoryID == id)).ToList();
        int balancePerOther = appliedChange / others.Count;
        foreach (var catId in others)
        {
            userScores[catId] -= balancePerOther;
        }

        //--in case the score of any gets <0
        int totalDebt = 0;
        foreach (var catId in userScores.Keys.ToList())
        {
            if (userScores[catId] < 0)
            {
                totalDebt += Math.Abs(userScores[catId]);
                userScores[catId] = 0;
            }
        }

        //--redistribute the score
        var healthyCategories = userScores.Where(x => x.Value > 0).Select(x => x.Key).ToList();
        while (totalDebt > 0 && healthyCategories.Count > 0)
        {
            foreach (var catId in healthyCategories.ToList())
            {
                if (totalDebt <= 0) break;
                if (userScores[catId] > 0)
                {
                    userScores[catId]--;
                    totalDebt--;
                }
                else { healthyCategories.Remove(catId); }
            }
        }

        //--final paranoia
        int currentSum = userScores.Values.Sum();
        if (currentSum != 10000)
        {
            int diff = 10000 - currentSum;
            int topCat = userScores.OrderByDescending(x => x.Value).First().Key;
            userScores[topCat] += diff;
        }

        return userScores;
    }

    internal int CalculateManhattanDistance(Dictionary<int, int> userScores, Post post, List<int> allCategoryIds)
    {
        int totalDistance = 0;

        var postCategories = new Dictionary<int, int>();
        for (int i = 0; i < post.Tags.Count; i++)
        {
            int catId = post.Tags[i].CategoryBelongingTo.CategoryID;

            int baseWeight = (10 - i) * 10;
            int weightedInfluence = baseWeight * 100;

            if (postCategories.ContainsKey(catId)) postCategories[catId] += weightedInfluence;
            else postCategories[catId] = weightedInfluence;
        }

        foreach (int catId in allCategoryIds)
        {
            int user_category = userScores.GetValueOrDefault(catId, 0);
            int post_category = postCategories.GetValueOrDefault(catId, 0);

            totalDistance += Math.Abs(user_category - post_category);
        }

        return totalDistance;
    }


}
