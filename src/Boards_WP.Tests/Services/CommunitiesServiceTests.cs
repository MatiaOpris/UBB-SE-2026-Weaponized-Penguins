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
    }
}