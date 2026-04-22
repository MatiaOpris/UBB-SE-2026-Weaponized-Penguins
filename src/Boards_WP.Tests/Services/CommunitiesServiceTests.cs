using System;
using System.Collections.Generic;

using Boards_WP.Data.Models;
using Boards_WP.Data.Services;
using Boards_WP.Data.Services.Interfaces;

using Moq;

using Xunit;

using IPostsService = Boards_WP.Data.Services.Interfaces.IPostsService;

namespace Boards_WP.Tests.Services
{
    public class CommunitiesServiceTests
    {
        private readonly Mock<ICommunitiesRepository> _repo;
        private readonly Mock<IPostsService> _posts;
        private readonly CommunitiesService _sut;

        public CommunitiesServiceTests()
        {
            _repo = new Mock<ICommunitiesRepository>();
            _posts = new Mock<IPostsService>();

            _sut = new CommunitiesService(_repo.Object, _posts.Object);
        }

        private static Community Community() =>
            new() { Name = "Test", Description = "Desc", Admin = new User { UserID = 1 } };

        // ───────── AddCommunity ─────────

        [Fact]
        public void AddCommunity_Valid_Adds()
        {
            var c = Community();
            _repo.Setup(x => x.AddCommunity(c)).Returns(1);

            _sut.AddCommunity(c);

            _repo.Verify(x => x.AddUserToCommunity(1, 1), Times.Once);
        }

        // ───────── DetermineTheme ─────────

        [Fact]
        public void DetermineTheme_WithPosts_CallsDominantColor()
        {
            var posts = new List<Post> { new() };
            _posts.Setup(x => x.GetPostsByCommunityIDs(It.IsAny<int[]>(), 0, 15)).Returns(posts);

            _sut.DetermineCommunityThemeColor(1);

            _posts.Verify(x => x.CalculateDominantColor(It.IsAny<IEnumerable<Post>>()), Times.Once);
        }

        // ───────── RemoveUser ─────────

        [Fact]
        public void RemoveUser_CallsCheckOwner()
        {
            _repo.Setup(x => x.CheckOwner(1, 2)).Returns(false);

            _sut.RemoveUser(1, 2);

            _repo.Verify(x => x.CheckOwner(1, 2), Times.Once);
        }

        // ───────── UpdateCommunityInfo ─────────

        [Fact]
        public void UpdateCommunityInfo_AllNull_DoesNothing()
        {
            _sut.UpdateCommunityInfo(1, null, null, null);

            _repo.Verify(x => x.UpdateDescription(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        // ───────── GetCommunityByID ─────────

        [Fact]
        public void GetCommunityByID_ReturnsCommunity()
        {
            _repo.Setup(x => x.GetCommunityByID(1)).Returns(Community());

            var result = _sut.GetCommunityByID(1);

            Assert.NotNull(result);
        }
        
        [Fact]
        public void AddCommunity_EmptyName_Throws()
        {
            var c = new Community { Name = "", Description = "desc", Admin = new User { UserID = 1 } };
        
            Assert.Throws<Exception>(() => _sut.AddCommunity(c));
        }
        
        [Fact]
        public void AddUser_RepoThrows_Throws()
        {
            _repo.Setup(x => x.AddUserToCommunity(1, 2)).Throws(new Exception("DB error"));
        
            Assert.Throws<Exception>(() => _sut.AddUser(1, 2));
        }
        
        [Fact]
        public void AddUser_Valid_CallsRepo()
        {
            _sut.AddUser(1, 2);
        
            _repo.Verify(x => x.AddUserToCommunity(1, 2), Times.Once);
            _repo.Verify(x => x.IncreaseMembersNumber(1), Times.Once);
        }
        
        [Fact]
        public void CheckOwner_RepoThrows_Throws()
        {
            _repo.Setup(x => x.CheckOwner(1, 2)).Throws(new Exception("DB error"));
        
            Assert.Throws<Exception>(() => _sut.CheckOwner(1, 2));
        }

        [Fact]
        public void CheckOwner_Valid_ReturnsTrue()
        {
            _repo.Setup(x => x.CheckOwner(1, 2)).Returns(true);

            var result = _sut.CheckOwner(1, 2);

            Assert.True(result);
        }
        
        [Fact]
        public void DetermineTheme_NoPosts_ReturnsDefault()
        {
            _posts.Setup(x => x.GetPostsByCommunityIDs(It.IsAny<int[]>(), 0, 15))
                .Returns(new List<Post>());
        
            var result = _sut.DetermineCommunityThemeColor(1);
        
            Assert.Equal(ThemeColor.Default, result);
        }
        
        [Fact]
        public void GetCommunitiesUserIsPartOf_RepoThrows_Throws()
        {
            _repo.Setup(x => x.GetCommunitiesUserIsPartOf(1)).Throws(new Exception("DB error"));
        
            Assert.Throws<Exception>(() => _sut.GetCommunitiesUserIsPartOf(1));
        }

        [Fact]
        public void GetCommunitiesUserIsPartOf_Valid_ReturnsList()
        {
            var communities = new List<Community> { Community() };
            _repo.Setup(x => x.GetCommunitiesUserIsPartOf(1)).Returns(communities);
            var result = _sut.GetCommunitiesUserIsPartOf(1);
        
            Assert.NotNull(result);
            Assert.Single(result);
        }
        
        [Fact]
        public void IsPartOfCommunity_RepoThrows_Throws()
        {
            _repo.Setup(x => x.IsPartOfCommunity(1, 2)).Throws(new Exception("DB error"));
            var result = Assert.Throws<Exception>(() => _sut.IsPartOfCommunity(1, 2));
            Assert.Equal("Failed to check if user is part of community.", result.Message);
        }
        
        [Fact]
        public void IsPartOfCommunity_Valid_ReturnsTrue()
        {
            _repo.Setup(x => x.IsPartOfCommunity(1, 2)).Returns(true);
            var result = _sut.IsPartOfCommunity(1, 2);
            Assert.True(result);
        }
        
        [Fact]
        public void RemoveUser_RepoThrows_Throws()
        {
            _repo.Setup(x => x.CheckOwner(1, 2)).Returns(false);
            _repo.Setup(x => x.RemoveUserFromCommunity(1, 2)).Throws(new Exception("DB error"));
        
            var result = Assert.Throws<Exception>(() => _sut.RemoveUser(1, 2));
            Assert.Equal("Failed to remove user from community.", result.Message);
        }
        
        [Fact]
        public void UpdateCommunityInfo_WithValues_CallsRepo()
        {
            _sut.UpdateCommunityInfo(1, "desc", new byte[] { 1 }, new byte[] { 2 });
        
            _repo.Verify(x => x.UpdateDescription(1, "desc"), Times.Once);
            _repo.Verify(x => x.UpdateCommunityPicture(1, It.IsAny<byte[]>()), Times.Once);
            _repo.Verify(x => x.UpdateBanner(1, It.IsAny<byte[]>()), Times.Once);
        }
   
        [Fact]
        public void UpdateCommunityInfo_RepoThrows_Throws()
        {
            _repo.Setup(x => x.UpdateDescription(1, "desc")).Throws(new Exception("DB error"));
        
            var result = Assert.Throws<Exception>(() => _sut.UpdateCommunityInfo(1, "desc", null, null));
            Assert.Equal("Failed to update community info.", result.Message);
        }
        
        [Fact]
        public void AddCommunity_NameTooLong_Throws()
        {
            var c = new Community
            {
                Name = new string('a', 201),
                Description = "desc",
                Admin = new User { UserID = 1 }
            };

            Assert.Throws<Exception>(() => _sut.AddCommunity(c));
        }

        [Fact]
        public void AddCommunity_EmptyDescription_Throws()
        {
            var c = new Community
            {
                Name = "ok",
                Description = "",
                Admin = new User { UserID = 1 }
            };

            Assert.Throws<Exception>(() => _sut.AddCommunity(c));
        }

        [Fact]
        public void AddCommunity_DescriptionTooLong_Throws()
        {
            var c = new Community
            {
                Name = "ok",
                Description = new string('a', 501),
                Admin = new User { UserID = 1 }
            };

            Assert.Throws<Exception>(() => _sut.AddCommunity(c));
        }
        
        [Fact]
        public void GetCommunityByID_RepositoryThrows_Throws()
        {
            _repo.Setup(x => x.GetCommunityByID(1))
                .Throws(new Exception());

            Assert.Throws<Exception>(() => _sut.GetCommunityByID(1));
        }
        
        [Fact]
        public void SearchCommunities_RepositoryThrows_Throws()
        {
            _repo.Setup(x => x.GetCommunitiesByNamesMatch("test"))
                .Throws(new Exception());

            Assert.Throws<Exception>(() => _sut.searchCommunities("test"));
        }
        
        [Fact]
        public void SearchCommunities_Valid_ReturnsList()
        {
            var communities = new List<Community> { Community() };
            _repo.Setup(x => x.GetCommunitiesByNamesMatch("test")).Returns(communities);

            var result = _sut.searchCommunities("test");

            Assert.NotNull(result);
            Assert.Single(result);
        }
    }
}