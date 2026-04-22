using System;
using System.Collections.Generic;

using Moq;

using Xunit;

using Boards_WP.Data.Models;
using Boards_WP.Data.Services;
using Boards_WP.Data.Repositories;

namespace Boards_WP.Tests.Services
{
    public class PostsServiceTests
    {
        private readonly Mock<IPostsRepository> _postsRepoMock;
        private readonly Mock<IUsersRepository> _usersRepoMock;
        private readonly Mock<ITagsRepository> _tagsRepoMock;
        private readonly Mock<IUsersMoodRepository> _usersMoodRepoMock;
        private readonly Mock<ICommunitiesRepository> _communitiesRepoMock;

        private readonly UserSession _userSession;
        private readonly PostsService _service;

        public PostsServiceTests()
        {
            _postsRepoMock = new Mock<IPostsRepository>();
            _usersRepoMock = new Mock<IUsersRepository>();
            _tagsRepoMock = new Mock<ITagsRepository>();
            _usersMoodRepoMock = new Mock<IUsersMoodRepository>();
            _communitiesRepoMock = new Mock<ICommunitiesRepository>();

            _userSession = new UserSession
            {
                CurrentUser = new User
                {
                    UserID = 1,
                    Username = "test_user"
                }
            };

            _service = new PostsService(
                _postsRepoMock.Object,
                _usersRepoMock.Object,
                _tagsRepoMock.Object,
                _userSession,
                _usersMoodRepoMock.Object,
                _communitiesRepoMock.Object);
        }

        private static Post CreateValidPost(
            int ownerId = 1,
            int adminId = 10,
            int categoryId = 1,
            string title = "Valid title",
            string description = "Valid description")
        {
            return new Post
            {
                PostID = 100,
                Owner = new User { UserID = ownerId },
                ParentCommunity = new Community
                {
                    CommunityID = 1,
                    Admin = new User { UserID = adminId }
                },
                Title = title,
                Description = description,
                CommentsNumber = 0,
                Tags = new List<Tag>
                {
                    new Tag
                    {
                        CategoryBelongingTo = new Category
                        {
                            CategoryID = categoryId
                        }
                    }
                }
            };
        }

        private static string CreateString(int length)
        {
            return new string('a', length);
        }

        [Fact]
        public void ValidatePost_ValidPost_DoesNotThrow()
        {
            // Arrange
            var post = CreateValidPost();

            // Act
            var exception = Record.Exception(() => _service.ValidatePost(post));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void ValidatePost_NullPost_ThrowsArgumentNullException()
        {
            // Arrange
            Post post = null!;

            // Act
            Action act = () => _service.ValidatePost(post);

            // Assert
            Assert.Throws<ArgumentNullException>(act);
        }

        [Fact]
        public void ValidatePost_NullOwner_ThrowsArgumentException()
        {
            // Arrange
            var post = CreateValidPost();
            post.Owner = null!;

            // Act
            Action act = () => _service.ValidatePost(post);

            // Assert
            Assert.Throws<ArgumentException>(act);
        }

        [Fact]
        public void ValidatePost_NullCommunity_ThrowsArgumentException()
        {
            // Arrange
            var post = CreateValidPost();
            post.ParentCommunity = null!;

            // Act
            Action act = () => _service.ValidatePost(post);

            // Assert
            Assert.Throws<ArgumentException>(act);
        }

        [Fact]
        public void ValidatePost_EmptyTitle_ThrowsArgumentException()
        {
            // Arrange
            var post = CreateValidPost(title: " ");

            // Act
            Action act = () => _service.ValidatePost(post);

            // Assert
            Assert.Throws<ArgumentException>(act);
        }

        [Fact]
        public void ValidatePost_TitleLongerThan100_ThrowsArgumentException()
        {
            // Arrange
            var post = CreateValidPost(title: CreateString(101));

            // Act
            Action act = () => _service.ValidatePost(post);

            // Assert
            Assert.Throws<ArgumentException>(act);
        }

        [Fact]
        public void ValidatePost_DescriptionLongerThan3000_ThrowsArgumentException()
        {
            // Arrange
            var post = CreateValidPost(description: CreateString(3001));

            // Act
            Action act = () => _service.ValidatePost(post);

            // Assert
            Assert.Throws<ArgumentException>(act);
        }

        [Fact]
        public void ValidatePost_MoreThan10Tags_ThrowsArgumentException()
        {
            // Arrange
            var post = CreateValidPost();
            post.Tags = new List<Tag>
            {
                new Tag { CategoryBelongingTo = new Category { CategoryID = 1 } },
                new Tag { CategoryBelongingTo = new Category { CategoryID = 2 } },
                new Tag { CategoryBelongingTo = new Category { CategoryID = 3 } },
                new Tag { CategoryBelongingTo = new Category { CategoryID = 4 } },
                new Tag { CategoryBelongingTo = new Category { CategoryID = 5 } },
                new Tag { CategoryBelongingTo = new Category { CategoryID = 6 } },
                new Tag { CategoryBelongingTo = new Category { CategoryID = 7 } },
                new Tag { CategoryBelongingTo = new Category { CategoryID = 8 } },
                new Tag { CategoryBelongingTo = new Category { CategoryID = 9 } },
                new Tag { CategoryBelongingTo = new Category { CategoryID = 10 } },
                new Tag { CategoryBelongingTo = new Category { CategoryID = 11 } }
            };

            // Act
            Action act = () => _service.ValidatePost(post);

            // Assert
            Assert.Throws<ArgumentException>(act);
        }

        [Fact]
        public void ValidatePost_NegativeCommentsNumber_ThrowsArgumentException()
        {
            // Arrange
            var post = CreateValidPost();
            post.CommentsNumber = -1;

            // Act
            Action act = () => _service.ValidatePost(post);

            // Assert
            Assert.Throws<ArgumentException>(act);
        }

        [Fact]
        public void ValidatePost_ImageLargerThan10Mb_ThrowsArgumentException()
        {
            // Arrange
            var post = CreateValidPost();
            post.Image = new byte[10485761];

            // Act
            Action act = () => _service.ValidatePost(post);

            // Assert
            Assert.Throws<ArgumentException>(act);
        }

        [Fact]
        public void DeletePost_CurrentUserIsOwner_DeletesPost()
        {
            // Arrange
            var post = CreateValidPost(ownerId: 1, adminId: 99);

            _postsRepoMock
                .Setup(repo => repo.GetPostByPostID(100))
                .Returns(post);

            // Act
            _service.DeletePost(100);

            // Assert
            _postsRepoMock.Verify(repo => repo.DeletePost(100), Times.Once);
        }

        [Fact]
        public void DeletePost_CurrentUserIsCommunityAdmin_DeletesPost()
        {
            // Arrange
            var post = CreateValidPost(ownerId: 2, adminId: 1);

            _postsRepoMock
                .Setup(repo => repo.GetPostByPostID(100))
                .Returns(post);

            // Act
            _service.DeletePost(100);

            // Assert
            _postsRepoMock.Verify(repo => repo.DeletePost(100), Times.Once);
        }

        [Fact]
        public void DeletePost_CurrentUserIsNotOwnerOrAdmin_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var post = CreateValidPost(ownerId: 2, adminId: 3);

            _postsRepoMock
                .Setup(repo => repo.GetPostByPostID(100))
                .Returns(post);

            // Act
            Action act = () => _service.DeletePost(100);

            // Assert
            Assert.Throws<UnauthorizedAccessException>(act);
        }

        [Fact]
        public void IncreaseScore_CurrentVoteIsNone_SetsLikeAndIncreasesScoreOnce()
        {
            // Arrange
            var post = CreateValidPost();

            _postsRepoMock
                .Setup(repo => repo.GetUserVoteForPost(1, 100))
                .Returns(VoteType.None);

            _postsRepoMock
                .Setup(repo => repo.GetPostByPostID(100))
                .Returns(post);

            // Act
            _service.IncreaseScore(100);

            // Assert
            _postsRepoMock.Verify(repo => repo.SetUserVoteForPost(1, 100, VoteType.Like), Times.Once);
            _postsRepoMock.Verify(repo => repo.IncreaseScore(100), Times.Once);
        }

        [Fact]
        public void IncreaseScore_CurrentVoteIsLike_RemovesVoteAndDecreasesScoreOnce()
        {
            // Arrange
            _postsRepoMock
                .Setup(repo => repo.GetUserVoteForPost(1, 100))
                .Returns(VoteType.Like);

            // Act
            _service.IncreaseScore(100);

            // Assert
            _postsRepoMock.Verify(repo => repo.SetUserVoteForPost(1, 100, VoteType.None), Times.Once);
            _postsRepoMock.Verify(repo => repo.DecreaseScore(100), Times.Once);
        }

        [Fact]
        public void IncreaseScore_CurrentVoteIsDislike_SetsLikeAndIncreasesScoreTwice()
        {
            // Arrange
            var post = CreateValidPost();

            _postsRepoMock
                .Setup(repo => repo.GetUserVoteForPost(1, 100))
                .Returns(VoteType.Dislike);

            _postsRepoMock
                .Setup(repo => repo.GetPostByPostID(100))
                .Returns(post);

            // Act
            _service.IncreaseScore(100);

            // Assert
            _postsRepoMock.Verify(repo => repo.SetUserVoteForPost(1, 100, VoteType.Like), Times.Once);
            _postsRepoMock.Verify(repo => repo.IncreaseScore(100), Times.Exactly(2));
        }

        [Fact]
        public void DecreaseScore_CurrentVoteIsNone_SetsDislikeAndDecreasesScoreOnce()
        {
            // Arrange
            _postsRepoMock
                .Setup(repo => repo.GetUserVoteForPost(1, 100))
                .Returns(VoteType.None);

            // Act
            _service.DecreaseScore(100);

            // Assert
            _postsRepoMock.Verify(repo => repo.SetUserVoteForPost(1, 100, VoteType.Dislike), Times.Once);
            _postsRepoMock.Verify(repo => repo.DecreaseScore(100), Times.Once);
        }

        [Fact]
        public void DecreaseScore_CurrentVoteIsDislike_RemovesVoteAndIncreasesScoreOnce()
        {
            // Arrange
            _postsRepoMock
                .Setup(repo => repo.GetUserVoteForPost(1, 100))
                .Returns(VoteType.Dislike);

            // Act
            _service.DecreaseScore(100);

            // Assert
            _postsRepoMock.Verify(repo => repo.SetUserVoteForPost(1, 100, VoteType.None), Times.Once);
            _postsRepoMock.Verify(repo => repo.IncreaseScore(100), Times.Once);
        }

        [Fact]
        public void DecreaseScore_CurrentVoteIsLike_SetsDislikeAndDecreasesScoreTwice()
        {
            // Arrange
            _postsRepoMock
                .Setup(repo => repo.GetUserVoteForPost(1, 100))
                .Returns(VoteType.Like);

            // Act
            _service.DecreaseScore(100);

            // Assert
            _postsRepoMock.Verify(repo => repo.SetUserVoteForPost(1, 100, VoteType.Dislike), Times.Once);
            _postsRepoMock.Verify(repo => repo.DecreaseScore(100), Times.Exactly(2));
        }

        [Fact]
        public void DetermineThemeForASinglePost_NullPost_ReturnsDefault()
        {
            // Arrange
            Post post = null!;

            // Act
            var result = _service.DetermineThemeForASinglePost(post);

            // Assert
            Assert.Equal(ThemeColor.Default, result);
        }

        [Theory]
        [InlineData(1, ThemeColor.Pink)]
        [InlineData(4, ThemeColor.Orange)]
        public void DetermineThemeForASinglePost_PostWithSingleCategory_ReturnsMappedThemeColor(int categoryId, ThemeColor expectedColor)
        {
            // Arrange
            var post = CreateValidPost(categoryId: categoryId);

            // Act
            var result = _service.DetermineThemeForASinglePost(post);

            // Assert
            Assert.Equal(expectedColor, result);
        }

        [Fact]
        public void CalculateDominantColor_NoTags_ReturnsDefault()
        {
            // Arrange
            var posts = new List<Post>
            {
                new Post { Tags = null! },
                new Post { Tags = new List<Tag>() }
            };

            // Act
            var result = _service.CalculateDominantColor(posts);

            // Assert
            Assert.Equal(ThemeColor.Default, result);
        }

        [Fact]
        public void CalculateDominantColor_MajorityWeightedCategory_ReturnsExpectedColor()
        {
            // Arrange
            var posts = new List<Post>
            {
                new Post
                {
                    Tags = new List<Tag>
                    {
                        new Tag { CategoryBelongingTo = new Category { CategoryID = 4 } }
                    }
                },
                new Post
                {
                    Tags = new List<Tag>
                    {
                        new Tag { CategoryBelongingTo = new Category { CategoryID = 4 } }
                    }
                },
                new Post
                {
                    Tags = new List<Tag>
                    {
                        new Tag { CategoryBelongingTo = new Category { CategoryID = 1 } }
                    }
                }
            };

            // Act
            var result = _service.CalculateDominantColor(posts);

            // Assert
            Assert.Equal(ThemeColor.Orange, result);
        }
    }
}