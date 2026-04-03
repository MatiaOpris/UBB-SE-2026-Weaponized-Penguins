using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Boards_WP.Data.Services;

public class PostsService : IPostsService
{
    private readonly IPostsRepository _postsRepo;
    private readonly IUsersRepository _usersRepo;
    private readonly ITagsRepository _tagsRepo;
    private readonly ICommunitiesRepository _communitiesRepo;
    private readonly UserSession _userSession;
    private readonly IUsersMoodRepository _usersMoodRepository;
    private int _cachedCategoryCount = 0;
    private readonly int _goldenPostsDiscoveryPercent = 25;
    private readonly int _badPostsDiscoveryPercent = 25;
    private readonly int _safetyDiscoveryThreshHold = 2500;
    private readonly int _safetyDiscoveryResetCount = 500;
    private readonly int _initialDiscoveryBatch = 1000;

    private List<Post> _discoveryBuffer = new();
    private List<Post> _discoveryKeptPool = new();
    private int _discoveryDbOffset = 0;

    private List<Post> _lastLikesOfCurrentUser= new List<Post>();  

    public PostsService(IPostsRepository postsRepo, IUsersRepository usersRepo, ITagsRepository tagsRepo, 
        UserSession userSession, IUsersMoodRepository usersMoodRepository, ICommunitiesRepository communitiesRepo)
    {
        _postsRepo = postsRepo;
        _usersRepo = usersRepo;
        _tagsRepo = tagsRepo;
        _userSession = userSession;
        _usersMoodRepository = usersMoodRepository;
        _communitiesRepo = communitiesRepo;
    }


    public void AddPost(Post post)
    {
        ValidatePost(post);

        _postsRepo.AddPost(post);
    }

    public void ValidatePost(Post post)
    {
        if (post == null)
            throw new ArgumentNullException(nameof(post));

        if (post.Owner == null)
            throw new ArgumentException("A post must have an Owner.", nameof(post));

        if (post.ParentCommunity == null)
            throw new ArgumentException("A post must belong to a Community.", nameof(post));

        if (string.IsNullOrWhiteSpace(post.Title))
            throw new ArgumentException("Title is required.", nameof(post));

        if (post.Title.Length > 100)
            throw new ArgumentException("Title cannot exceed 100 characters.", nameof(post));

        if (!string.IsNullOrWhiteSpace(post.Description) && post.Description.Length > 3000)
            throw new ArgumentException("Description cannot exceed 3000 characters.", nameof(post));

        if (post.Tags != null && post.Tags.Count > 10)
            throw new ArgumentException("A post cannot have more than 10 tags.", nameof(post));

        if (post.CommentsNumber < 0)
            throw new ArgumentException("Comments number cannot be negative.", nameof(post));

        if (post.Image != null && post.Image.Length > 10485760)
            throw new ArgumentException("Image cannot exceed 10 MB.", nameof(post));
    }

    public Post GetPostByPostID(int postId)
    {
        return _postsRepo.GetPostByPostID(postId);
    }

    public void DeletePost(int postId)
    {
        var post = _postsRepo.GetPostByPostID(postId);
        int currentUserId = _userSession.CurrentUser.UserID;

        if (currentUserId == post.Owner.UserID)
            _postsRepo.DeletePost(postId);
        else if (currentUserId == post.ParentCommunity.Admin.UserID)
            _postsRepo.DeletePost(postId);
        else
            throw new UnauthorizedAccessException("Only the owner of the post can delete it.");
    }

    public void IncreaseCommentsNumber(int postId)
    {
        _postsRepo.IncreaseCommentsNumber(postId);
    }

    public void IncreaseScore(int postId)
    {
        
        int userId = _userSession.CurrentUser.UserID;
        VoteType currentVote = _postsRepo.GetUserVoteForPost(userId, postId);

        if (currentVote == VoteType.Like)
        {
            
            _postsRepo.SetUserVoteForPost(userId, postId, VoteType.None);
            _postsRepo.DecreaseScore(postId);

            _lastLikesOfCurrentUser.RemoveAll(p => p.PostID == postId);

        }
        else if (currentVote == VoteType.Dislike)
        {
            
            _postsRepo.SetUserVoteForPost(userId, postId, VoteType.Like);

            _postsRepo.IncreaseScore(postId);
            _postsRepo.IncreaseScore(postId);

            Post likedPost = _postsRepo.GetPostByPostID(postId);
            _lastLikesOfCurrentUser.Insert(0, likedPost);

            if (_lastLikesOfCurrentUser.Count > 5)
                _lastLikesOfCurrentUser.RemoveAt(_lastLikesOfCurrentUser.Count - 1);
        }
        else
        { 
            _postsRepo.SetUserVoteForPost(userId, postId, VoteType.Like);
            _postsRepo.IncreaseScore(postId);

            
            Post likedPost = _postsRepo.GetPostByPostID(postId);
            _lastLikesOfCurrentUser.Insert(0, likedPost);

            if (_lastLikesOfCurrentUser.Count > 5)
                _lastLikesOfCurrentUser.RemoveAt(_lastLikesOfCurrentUser.Count - 1);
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

            _lastLikesOfCurrentUser.RemoveAll(p => p.PostID == postId);
        }
        else
        {
            _postsRepo.SetUserVoteForPost(userId, postId, VoteType.Dislike);
            _postsRepo.DecreaseScore(postId);

            _lastLikesOfCurrentUser.RemoveAll(p => p.PostID == postId);
        }
    }

    public List<Post> GetPostsByCommunityIDs(int[] communityIds, int offset, int limit)
    {
        return _postsRepo.GetPostsByCommunityIDs(communityIds, offset, limit).OrderByDescending(post => post.CreationTime).ToList();
    }

    public List<Post> GetPostsForHomePage(int userId, int offset, int limit)
    {
        
        List<Community> communities = _communitiesRepo.GetCommunitiesUserIsPartOf(userId);

        int[] communityIds = communities.Select(c => c.CommunityID).ToArray();

        return _postsRepo.GetPostsByCommunityIDs(communityIds, offset, limit).OrderByDescending(post => post.CreationTime).ToList(); 
    }

    public List<Post> GetPostsForDiscoveryPage(int userId, int offset, int limit)
    {
        if (offset == 0)
        {
            _discoveryBuffer.Clear();
            _discoveryKeptPool.Clear();
            _discoveryDbOffset = 0;
        }

        if (_discoveryBuffer.Count < (offset + limit))
        {
            RunDiscoveryAlgorithmCycle(userId);
        }

        return _discoveryBuffer.Skip(offset).Take(limit).ToList();
    }

    private void RunDiscoveryAlgorithmCycle(int userId)
    {
        List<Community> communities = _communitiesRepo.GetCommunitiesUserIsPartOf(userId);
        int[] communityIds = communities.Select(c => c.CommunityID).ToArray();

        var freshPosts = _postsRepo.GetPostExceptCommunityIDs(communityIds, _discoveryDbOffset, _initialDiscoveryBatch);
        _discoveryDbOffset += _initialDiscoveryBatch;

        if (freshPosts.Count == 0 && _discoveryKeptPool.Count == 0) return;

        var totalPool = new List<Post>(_discoveryKeptPool);
        totalPool.AddRange(freshPosts);

        if (_cachedCategoryCount == 0)
            _cachedCategoryCount = _tagsRepo.GetCategoryCount();
        var userScores = _usersMoodRepository.GetUsersMoodScores(userId, _cachedCategoryCount);
        List<int> allCategoryIds = _tagsRepo.GetAllCategories().Select(c => c.CategoryID).ToList();
        var rankedPool = totalPool.Select(post => new
        {
            OriginalPost = post,
            RankScore = CalculateManhattanDistance(userScores, post, allCategoryIds)
        })
        .OrderBy(temp => temp.RankScore)
        .ThenByDescending(temp => temp.OriginalPost.CreationTime)
        .Select(temp => temp.OriginalPost)
        .ToList();

        int totalCount = rankedPool.Count;
        int topCount = totalCount * _goldenPostsDiscoveryPercent / 100;
        int middleCount = totalCount * (100 - _goldenPostsDiscoveryPercent - _badPostsDiscoveryPercent) / 100;

        _discoveryBuffer.AddRange(rankedPool.Take(topCount));
        var nextKeptPool = rankedPool.Skip(topCount).Take(middleCount).ToList();

        if (_discoveryBuffer.Count + nextKeptPool.Count >= _safetyDiscoveryThreshHold)
        {
            var elitePool = _discoveryBuffer.Concat(nextKeptPool)
                .Select(p => new { Post = p, Score = CalculateManhattanDistance(userScores, p, allCategoryIds) })
                .OrderBy(x => x.Score)
                .Take(_safetyDiscoveryResetCount)
                .Select(x => x.Post)
                .ToList();

            int resetTopCount = (_safetyDiscoveryResetCount * _goldenPostsDiscoveryPercent) / 100;
            int resetMidCount = (_safetyDiscoveryResetCount * (100 - _goldenPostsDiscoveryPercent - _badPostsDiscoveryPercent)) / 100;

            _discoveryBuffer = elitePool.Take(resetTopCount).ToList();
            _discoveryKeptPool = elitePool.Skip(resetTopCount).Take(resetMidCount).ToList();
        }
        else
        {
            _discoveryKeptPool = nextKeptPool;
        }
    }

    public ThemeColor DetermineThemeForASinglePost(Post post)
    {
        if (post == null) return ThemeColor.Default;
        return CalculateDominantColor(new[] { post });
    }

    public ThemeColor DetermineFeedThemeColorByLastLikes()
    {
        if (_lastLikesOfCurrentUser == null || !_lastLikesOfCurrentUser.Any())
            return ThemeColor.Default;
        var relevantPosts = _lastLikesOfCurrentUser.Take(5);

        return CalculateDominantColor(relevantPosts);
    }

    private readonly int[] _tagWeights = { 34, 21, 13, 8, 5, 3, 2, 1, 1, 1 };

    public ThemeColor CalculateDominantColor(IEnumerable<Post> posts)
    {
        
        var colorScores = new Dictionary<ThemeColor, int>
        {
            { ThemeColor.Pink, 0 },
            { ThemeColor.Orange, 0 },
            { ThemeColor.Turquoise, 0 },
            { ThemeColor.Yellow, 0 },
            { ThemeColor.Blue, 0 },
            { ThemeColor.Green, 0 },
            { ThemeColor.Red, 0 },
            { ThemeColor.Purple, 0 }
        };

            foreach (var post in posts)
            {
                if (post.Tags == null) continue;

                for (int i = 0; i < post.Tags.Count; i++)
                {
                    
                    int weight = (i < _tagWeights.Length) ? _tagWeights[i] : 1;

                    int categoryId = post.Tags[i].CategoryBelongingTo.CategoryID;
                    ThemeColor tagColor = CategoryThemeMapper.GetColorForCategoryId(categoryId);

                   
                    if (tagColor != ThemeColor.Default && colorScores.ContainsKey(tagColor))
                        colorScores[tagColor] += weight;
                    
                }
            }

        
        var winningColor = colorScores.OrderByDescending(kvp => kvp.Value).First();

        return winningColor.Value > 0 ? winningColor.Key : ThemeColor.Default;
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
