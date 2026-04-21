using System;
using System.Collections.Generic;

using Boards_WP.Data.Models;
using Boards_WP.Data.Repositories;
using Boards_WP.Data.Services;
using Boards_WP.Data.Services.Interfaces;

using Moq;

using Xunit;

// Alias to resolve interface in the correct namespace
using IUsersService = Boards_WP.Data.Services.Interfaces.IUsersService;
using ICommentsService = Boards_WP.Data.Services.Interfaces.ICommentsService;
using IPostsService = Boards_WP.Data.Services.Interfaces.IPostsService;


namespace Boards_WP.Tests.Services
{
    public class BetsServiceTests
    {
        private readonly Mock<IBetsRepository> _betsRepoMock;
        private readonly Mock<IUsersService> _usersServiceMock;
        private readonly Mock<ICommentsService> _commentsServiceMock;
        private readonly Mock<IPostsService> _postsServiceMock;
        private readonly BetsService _sut;

        public BetsServiceTests()
        {
            _betsRepoMock = new Mock<IBetsRepository>();
            _usersServiceMock = new Mock<IUsersService>();
            _commentsServiceMock = new Mock<ICommentsService>();
            _postsServiceMock = new Mock<IPostsService>();

            _sut = new BetsService(
                _betsRepoMock.Object,
                _usersServiceMock.Object,
                _commentsServiceMock.Object,
                _postsServiceMock.Object);
        }

        // ───────── HELPERS ─────────

        private static User CreateUser(int id = 1) => new() { UserID = id };

        private static Community CreateCommunity() =>
            new() { CommunityID = 1, Admin = CreateUser() };

        private static Bet CreateBet(
            int yes = 0, int no = 0,
            BetType type = BetType.Post,
            string expr = "key",
            DateTime? start = null,
            DateTime? end = null)
        {
            return new Bet
            {
                BetID = 1,
                BetCommunity = CreateCommunity(),
                YesAmount = yes,
                NoAmount = no,
                Type = type,
                Expression = expr,
                StartingTime = start ?? DateTime.Now.AddDays(-1),
                EndingTime = end ?? DateTime.Now.AddDays(1)
            };
        }

        private static UsersTokens Tokens(int amount, DateTime? last = null) =>
            new() { TokensNumber = amount, LastSeen = last ?? DateTime.Now };

        private static UsersBets UserBet(BetVote vote = BetVote.YES) =>
            new() { Vote = vote, Amount = 100, SelectedBet = CreateBet(), BettingUser = CreateUser() };

        // ───────── ExtractBetKeywords ─────────

        [Theory]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData("word", "")]
        [InlineData("cmd hello world", "hello world")]
        public void ExtractBetKeywords_VariousInputs_ReturnsExpected(string input, string expected)
        {
            // Act
            var result = _sut.ExtractBetKeywords(input);

            // Assert
            Assert.Equal(expected, result);
        }

        // ───────── IsSecretKey ─────────

        [Theory]
        [InlineData("/weaponizedpenguins", true)]
        [InlineData("/weaponizedpenguins extra", true)]
        [InlineData("wrong", false)]
        public void IsSecretKey_Input_ReturnsExpected(string input, bool expected)
        {
            Assert.Equal(expected, _sut.IsSecretKey(input));
        }

        // ───────── ValidatePlaceUserBet ─────────

        [Fact]
        public void ValidatePlaceUserBet_Valid_ReturnsTrue()
        {
            _betsRepoMock.Setup(x => x.GetUserTokens(1)).Returns(Tokens(500));

            Assert.True(_sut.ValidatePlaceUserBet(1, 100));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ValidatePlaceUserBet_InvalidAmount_Throws(int amount)
        {
            _betsRepoMock.Setup(x => x.GetUserTokens(1)).Returns(Tokens(500));

            Assert.Throws<Exception>(() => _sut.ValidatePlaceUserBet(1, amount));
        }

        [Fact]
        public void ValidatePlaceUserBet_NotEnoughTokens_Throws()
        {
            _betsRepoMock.Setup(x => x.GetUserTokens(1)).Returns(Tokens(10));

            Assert.Throws<Exception>(() => _sut.ValidatePlaceUserBet(1, 100));
        }

        // ───────── ValidateCreateBet ─────────

        [Fact]
        public void ValidateCreateBet_Valid_ReturnsTrue()
        {
            _betsRepoMock.Setup(x => x.GetUserTokens(1)).Returns(Tokens(10));

            var result = _sut.ValidateCreateBet(1, CreateBet(expr: "valid"));

            Assert.True(result);
        }

        [Fact]
        public void ValidateCreateBet_InvalidTimes_Throws()
        {
            _betsRepoMock.Setup(x => x.GetUserTokens(1)).Returns(Tokens(10));

            var bet = CreateBet(start: DateTime.Now, end: DateTime.Now);

            Assert.Throws<Exception>(() => _sut.ValidateCreateBet(1, bet));
        }

        // ───────── GetUserTokenFeeDiscount ─────────

        [Theory]
        [InlineData(0, 0)]
        [InlineData(5000, 0.25)]
        [InlineData(10000, 0.5)]
        public void GetUserTokenFeeDiscount_ReturnsExpected(int tokens, decimal expected)
        {
            _betsRepoMock.Setup(x => x.GetUserTokens(1)).Returns(Tokens(tokens));

            var result = _sut.GetUserTokenFeeDiscount(1);

            Assert.Equal(expected, result);
        }

        // ───────── CalculateBetOdds ─────────

        [Fact]
        public void CalculateBetOdds_SkewedYes_ReturnsDifferentOdds()
        {
            _betsRepoMock.Setup(x => x.GetBetByID(1)).Returns(CreateBet(2000, 100));
            _betsRepoMock.Setup(x => x.GetUserTokens(1)).Returns(Tokens(0));

            var (yes, no) = _sut.CalculateBetOdds(1, 1);

            Assert.NotEqual(yes, no);
        }

        // ───────── CheckBetCondition ─────────

        [Fact]
        public void CheckBetCondition_PostOutsideInterval_ReturnsNo()
        {
            var bet = CreateBet();
            _betsRepoMock.Setup(x => x.GetBetByID(1)).Returns(bet);

            _postsServiceMock.Setup(x => x.GetPostsByCommunityIDs(It.IsAny<int[]>(), 0, int.MaxValue))
                .Returns(new List<Post>
                {
                    new() { CreationTime = DateTime.Now.AddDays(-10), Title = "key" }
                });

            var result = _sut.CheckBetCondition(1);

            Assert.Equal(BetVote.NO, result);
        }

        // ───────── CreateBet ─────────

        [Fact]
        public void CreateBet_RepositoryThrows_DoesNotThrow()
        {
            var bet = CreateBet();

            _betsRepoMock.Setup(x => x.AddBet(bet)).Throws(new SqlException());

            _sut.CreateBet(bet, 1);
        }

        // ───────── ExecuteActionsByBetResult ─────────

        [Fact]
        public void ExecuteActionsByBetResult_NullUserBet_Throws()
        {
            _betsRepoMock.Setup(x => x.GetUserBetByID(1, 1)).Returns((UsersBets)null);

            Assert.Throws<Exception>(() => _sut.ExecuteActionsByBetResult(1, 1));
        }

        // ───────── RegisterSecretAreaVisit ─────────

        [Fact]
        public void RegisterSecretAreaVisit_NewUser_ReturnsFive()
        {
            _betsRepoMock.Setup(x => x.UserTokensExist(1)).Returns(false);
            _usersServiceMock.Setup(x => x.GetUserByID(1)).Returns(CreateUser());

            var result = _sut.RegisterSecretAreaVisitAndGetTokens(1);

            Assert.Equal(5, result);
        }

        // ───────── GetBetsOfUser ─────────

        [Fact]
        public void GetBetsOfUser_NullSelectedBet_Ignored()
        {
            _betsRepoMock.Setup(x => x.GetUserBetsByUser(1))
                .Returns(new List<UsersBets>
                {
                    new UsersBets { SelectedBet = null }
                });

            var result = _sut.GetBetsOfUser(1);

            Assert.Empty(result);
        }

        // ───────── didUserWinBet ─────────

        [Fact]
        public void didUserWinBet_NullUserBet_Throws()
        {
            _betsRepoMock.Setup(x => x.GetUserBetByID(1, 1)).Returns((UsersBets)null);

            Assert.Throws<Exception>(() => _sut.didUserWinBet(1, 1));
        }
    }
}