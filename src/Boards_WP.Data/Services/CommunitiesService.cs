using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Boards_WP.Data.Services;

public class CommunitiesService : ICommunitiesService
{
    private readonly ICommunitiesRepository _communitiesRepo;
    private readonly IPostsService _postsService;
    public CommunitiesService(ICommunitiesRepository communitiesRepo, IPostsService postsService)
    {
        _communitiesRepo = communitiesRepo;
        _postsService = postsService;
    }
    public void AddCommunity(Community AddedCommunity)
    {
        try
        {
            validateCommunity(AddedCommunity);
            AddedCommunity.CommunityID = _communitiesRepo.AddCommunity(AddedCommunity);
            _communitiesRepo.AddUserToCommunity(AddedCommunity.CommunityID, AddedCommunity.Admin.UserID);
            _communitiesRepo.IncreaseMembersNumber(AddedCommunity.CommunityID);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to add community: {ex.Message}");
        }
    }

    public void AddUser(int CommunityID, int UserID)
    {
        try
        {
            _communitiesRepo.AddUserToCommunity(CommunityID, UserID);
            _communitiesRepo.IncreaseMembersNumber(CommunityID);
        }
        catch
        {
            throw new Exception("Failed to add user to community.");
        }
    }

    public bool CheckOwner(int CommunityID, int UserID)
    {
        try
        {
            return _communitiesRepo.CheckOwner(CommunityID, UserID);

        }
        catch
        {
            throw new Exception("Failed to check if user is owner of community.");
        }
    }

    public ThemeColor DetermineCommunityThemeColor(int communityId)
    {

        var recentPosts = _postsService.GetPostsByCommunityIDs(new[] { communityId }, 0, 15).OrderByDescending(post => post.CreationTime).ToList(); ;

        if (recentPosts == null || !recentPosts.Any())
        {
            return ThemeColor.Default;
        }
        return _postsService.CalculateDominantColor(recentPosts);
    }

    public List<Community> GetCommunitiesUserIsPartOf(int UserID)
    {
        try
        {
            return _communitiesRepo.GetCommunitiesUserIsPartOf(UserID);
        }
        catch
        {
            throw new Exception("Failed to get communities user is part of.");
        }
    }


    public bool IsPartOfCommunity(int UserID, int CommunityID)
    {
        try
        {
            return _communitiesRepo.IsPartOfCommunity(UserID, CommunityID);

        }
        catch
        {
            throw new Exception("Failed to check if user is part of community.");
        }
    }

    public void RemoveUser(int CommunityID, int UserID)
    {
        try
        {
            if (_communitiesRepo.CheckOwner(CommunityID, UserID) == false)
            {
                _communitiesRepo.RemoveUserFromCommunity(CommunityID, UserID);
                _communitiesRepo.DecreaseMembersNumber(CommunityID);
            }

        }
        catch
        {
            throw new Exception("Failed to remove user from community.");
        }
    }

    public List<Community> searchCommunities(string Match)
    {
        try
        {
            return _communitiesRepo.GetCommunitiesByNamesMatch(Match);
        }
        catch
        {
            throw new Exception("Failed to search communities.");
        }
    }

    public void UpdateCommunityInfo(int CommunityID, string Description, byte[] NewCommunityPicture, byte[] NewBanner)
    {
        try
        {
            if (Description != null)
                _communitiesRepo.UpdateDescription(CommunityID, Description);
            if (NewCommunityPicture != null)
                _communitiesRepo.UpdateCommunityPicture(CommunityID, NewCommunityPicture);
            if (NewBanner != null)
                _communitiesRepo.UpdateBanner(CommunityID, NewBanner);
        }
        catch
        {
            throw new Exception("Failed to update community info.");
        }
    }

    private void validateCommunity(Community c)
    {
        if (string.IsNullOrEmpty(c.Name))
            throw new Exception("Community name cannot be empty.");
        if (c.Name.Length > 200)
            throw new Exception("Community name cannot exceed 200 characters.");
        if (string.IsNullOrEmpty(c.Description))
            throw new Exception("Community description cannot be empty.");
        if (c.Description.Length > 500)
            throw new Exception("Community description cannot exceed 500 characters.");
    }

    public Community GetCommunityByID(int CommunityID)
    {
        try
        {
            return _communitiesRepo.GetCommunityByID(CommunityID);
        }
        catch
        {
            throw new Exception("Failed to get community by ID.");
        }
    }
}
