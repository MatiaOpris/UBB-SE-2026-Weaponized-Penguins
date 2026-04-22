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
            new() { CurrentUser = CreateUser(), TokensNumber = amount, LastSeen = last ?? DateTime.Now };

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

            var bet = CreateBet(
                start: DateTime.Now,
                end: DateTime.Now.AddMinutes(-1)
            );

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
        
        [Fact]
        public void CalculateBetOdds_FailingToGetBet_Throws()
        {
            _betsRepoMock.Setup(x => x.GetBetByID(1)).Returns((Bet)null);

            Assert.Throws<Exception>(() => _sut.CalculateBetOdds(1, 1));
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
        
        [Fact]
        public void CheckBetCondition_PostMatch_ReturnsYes()
        {
            var bet = CreateBet();
            _betsRepoMock.Setup(x => x.GetBetByID(1)).Returns(bet);

            _postsServiceMock.Setup(x => x.GetPostsByCommunityIDs(It.IsAny<int[]>(), 0, int.MaxValue))
                .Returns(new List<Post>
                {
                    new() { CreationTime = DateTime.Now, Title = "key" }
                });

            var result = _sut.CheckBetCondition(1);

            Assert.Equal(BetVote.YES, result);
        }
        
        // ───────── CreateBet ─────────

        [Fact]
        public void CreateBet_RepositoryThrows_ThrowsException()
        {
            var bet = CreateBet();

            _betsRepoMock
                .Setup(x => x.AddBet(bet))
                .Throws(new Exception("DB error"));

            Assert.Throws<Exception>(() => _sut.CreateBet(bet, 1));
        }
        
        [Fact]
        public void CreateBet_Valid_CreatesBetAndUpdatesTokens()
        {
            var bet = CreateBet();
            _betsRepoMock.Setup(x => x.AddBet(bet)).Returns(1);
            _betsRepoMock.Setup(x => x.GetUserTokens(1)).Returns(Tokens(500));
            
            _sut.CreateBet(bet, 1);
            _betsRepoMock.Verify(x => x.AddBet(bet), Times.Once);
            _betsRepoMock.Verify(x => x.UpdateUserTokens(1, 495), Times.Once);
        }

        // ───────── ExecuteActionsByBetResult ─────────

        [Fact]
        public void ExecuteActionsByBetResult_NullUserBet_Throws()
        {
            _betsRepoMock.Setup(x => x.GetUserBetByID(1, 1)).Returns((UsersBets)null);

            Assert.Throws<Exception>(() => _sut.ExecuteActionsByBetResult(1, 1));
        }
        
        [Fact]
        public void ExecuteActionsByBetResult_UserWins_UpdatesTokens()
        {
            var userBet = UserBet(BetVote.YES);
            _betsRepoMock.Setup(x => x.GetUserBetByID(1, 1)).Returns(userBet);
            _betsRepoMock.Setup(x => x.GetUserTokens(1)).Returns(Tokens(500));
            _betsRepoMock.Setup(x => x.GetBetByID(1)).Returns(CreateBet(2000, 100));
            _postsServiceMock.Setup(x => x.GetPostsByCommunityIDs(It.IsAny<int[]>(), 0, int.MaxValue))
                .Returns(new List<Post> { new() { CreationTime = DateTime.Now, Title = "key" } });      
            
            _sut.ExecuteActionsByBetResult(1, 1);
            
            _betsRepoMock.Verify(x => x.UpdateUserTokens(1, It.Is<int>(tokens => tokens > 500)), Times.Once);
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
        
        [Fact]
        public void RegisterSecretAreaVisit_ExistingUser_ReturnsUpdatedTokens()
        {
            _betsRepoMock.Setup(x => x.UserTokensExist(1)).Returns(true);
            _betsRepoMock.Setup(x => x.GetUserTokens(1)).Returns(Tokens(10, DateTime.Now.AddDays(-2)));

            var result = _sut.RegisterSecretAreaVisitAndGetTokens(1);

            Assert.Equal(12, result);
            _betsRepoMock.Verify(x => x.UpdateUserTokens(1, 12), Times.Once);
        }
        
        [Fact]
        public void RegisterSecretAreaVisit_ExistingUserNoDaysPassed_ReturnsSameTokens()
        {
            _betsRepoMock.Setup(x => x.UserTokensExist(1)).Returns(true);
            _betsRepoMock.Setup(x => x.GetUserTokens(1)).Returns(Tokens(10, DateTime.Now));
            
            var result = _sut.RegisterSecretAreaVisitAndGetTokens(1);   
            
            Assert.Equal(10, result);
            _betsRepoMock.Verify(x => x.UpdateUserTokens(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }
        
        [Fact]
        public void RegisterSecretAreaVisit_UserNotFound_Throws()
        {
            _betsRepoMock.Setup(x => x.UserTokensExist(1)).Returns(false);
            _usersServiceMock.Setup(x => x.GetUserByID(1)).Returns((User)null);
            
            Assert.Throws<Exception>(() => _sut.RegisterSecretAreaVisitAndGetTokens(1));
        }

        // ───────── GetBetsOfUser ─────────

        [Fact]
        public void GetBetsOfUser_NullSelectedBet_Ignored()
        {
            _betsRepoMock.Setup(x => x.GetUserBetsByUser(1))
                .Returns(new List<UsersBets>
                {
                    new UsersBets { BettingUser = CreateUser(), SelectedBet = null }
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
        
        // ─── RegisterSecretArea ───
        
        [Fact]
        public void RegisterSecretArea_RepoThrows_Throws()
        {
            _betsRepoMock.Setup(x => x.UserTokensExist(1))
                .Throws(new Exception());

            Assert.Throws<Exception>(() => _sut.RegisterSecretAreaVisitAndGetTokens(1));
        }
        
        // ─── GetAllBets ───
        
        [Fact]
        public void GetAllBets_RepoThrows_Throws()
        {
            _betsRepoMock.Setup(x => x.GetAllBetsSortedByDate())
                .Throws(new Exception());

            Assert.Throws<Exception>(() => _sut.GetAllBets());
        }

        [Fact]
        public void GetAllBets_Valid_ReturnsBets()
        {
            var bets = new List<Bet> { CreateBet() };
            _betsRepoMock.Setup(x => x.GetAllBetsSortedByDate()).Returns(bets);

            var result = _sut.GetAllBets();

            Assert.Equal(bets, result);
        }
        
        // ─── GetBetsOfUsers ───
        
        [Fact]
        public void GetBetsOfUser_SkipsNullSelectedBet()
        {
            var userBets = new List<UsersBets>
            {
                new() { BettingUser = CreateUser(), SelectedBet = null },
                new() { BettingUser = CreateUser(), SelectedBet = CreateBet() }
            };
            _betsRepoMock.Setup(x => x.GetUserBetsByUser(1)).Returns(userBets);

            var result = _sut.GetBetsOfUser(1);

            Assert.Single(result);
        }
        
        [Fact]
        public void GetBetsOfUser_RepoThrows_Throws()
        {
            _betsRepoMock.Setup(x => x.GetUserBetsByUser(1))
                .Throws(new Exception());

            Assert.Throws<Exception>(() => _sut.GetBetsOfUser(1));
        }
        
        // ─── GetPlacedBetsOfUsers ───
        
        [Fact]
        public void GetPlacedBetsOfUser_RepoThrows_Throws()
        {
            _betsRepoMock.Setup(x => x.GetUserBetsByUser(1))
                .Throws(new Exception());

            Assert.Throws<Exception>(() => _sut.GetPlacedBetsOfUser(1));
        }
        
        [Fact]
        public void GetPlacedBetsOfUser_Valid_ReturnsBets()
        {
            var userBets = new List<UsersBets>
            {
                new() { BettingUser = CreateUser(), SelectedBet = CreateBet() },
                new() { BettingUser = CreateUser(), SelectedBet = CreateBet() }
            };
            _betsRepoMock.Setup(x => x.GetUserBetsByUser(1)).Returns(userBets);

            var result = _sut.GetPlacedBetsOfUser(1);

            Assert.Equal(userBets, result);
        }
        
        // ─── GetOngoingPlacedBetsOfUser ───
        [Fact]
        public void GetOngoingPlacedBetsOfUser_ReturnsOngoing()
        {
            var userBets = new List<UsersBets>
            {
                new() { BettingUser = CreateUser(), SelectedBet = CreateBet(end: DateTime.Now.AddDays(1)) },
                new() { BettingUser = CreateUser(), SelectedBet = CreateBet(end: DateTime.Now.AddDays(-1)) }
            };
            _betsRepoMock.Setup(x => x.GetUserBetsByUser(1)).Returns(userBets);

            var result = _sut.GetOngoingPlacedBetsOfUser(1);

            Assert.Single(result);
        }
        
        [Fact]
        public void GetOngoingPlacedBetsOfUser_RepoThrows_Throws()
        {
            _betsRepoMock.Setup(x => x.GetUserBetsByUser(1))
                .Throws(new Exception());

            Assert.Throws<Exception>(() => _sut.GetOngoingPlacedBetsOfUser(1));
        }
        
        // ─── GetExpiredPlacedBetsOfUser ───
        
        [Fact]
        public void GetExpiredPlacedBetsOfUser_ReturnsExpired()
        {
            var userBets = new List<UsersBets>
            {
                new() { BettingUser = CreateUser(), SelectedBet = CreateBet(end: DateTime.Now.AddDays(1)) },
                new() { BettingUser = CreateUser(), SelectedBet = CreateBet(end: DateTime.Now.AddDays(-1)) }
            };
            _betsRepoMock.Setup(x => x.GetUserBetsByUser(1)).Returns(userBets);

            var result = _sut.GetExpiredPlacedBetsOfUser(1);

            Assert.Single(result);
        }
        
        [Fact]
        public void GetExpiredPlacedBetsOfUser_RepoThrows_Throws()
        {
            _betsRepoMock.Setup(x => x.GetUserBetsByUser(1))
                .Throws(new Exception());

            Assert.Throws<Exception>(() => _sut.GetExpiredPlacedBetsOfUser(1));
        }
        
        // ─── GetBetById───
        
        [Fact]
        public void GetBetByID_RepoThrows_Throws()
        {
            _betsRepoMock.Setup(x => x.GetBetByID(1))
                .Throws(new Exception());

            Assert.Throws<Exception>(() => _sut.GetBetByID(1));
        }

        [Fact]
        public void GetBetByID_Valid_ReturnsBet()
        {
            var bet = CreateBet();
            _betsRepoMock.Setup(x => x.GetBetByID(1)).Returns(bet);

            var result = _sut.GetBetByID(1);

            Assert.Equal(bet, result);
        }
        
        // ─── GetExpiredBetsOfUser ───
        
        [Fact]
        public void GetExpiredBetsOfUser_ReturnsExpired()
        {
            var userBets = new List<UsersBets>
            {
                new() { BettingUser = CreateUser(), SelectedBet = CreateBet(end: DateTime.Now.AddDays(1)) },
                new() { BettingUser = CreateUser(), SelectedBet = CreateBet(end: DateTime.Now.AddDays(-1)) }
            };
            _betsRepoMock.Setup(x => x.GetUserBetsByUser(1)).Returns(userBets);

            var result = _sut.GetExpiredBetsOfUser(1);

            Assert.Single(result);
        }
        
        // ─── GetUserTokenCount ───
        
        [Fact]
        public void GetUserTokenCount_RepoThrows_Throws()
        {
            _betsRepoMock.Setup(x => x.GetUserTokens(1))
                .Throws(new Exception());

            Assert.Throws<Exception>(() => _sut.GetUserTokenCount(1));
        }

        [Fact]
        public void GetUserTokenCount_Valid_ReturnsCount()
        {
            _betsRepoMock.Setup(x => x.GetUserTokens(1)).Returns(Tokens(500));

            var result = _sut.GetUserTokenCount(1);

            Assert.Equal(500, result);
        }
        
        // ─── GetUserTokenFeeDiscount ───
        
        [Fact]
        public void GetUserTokenFeeDiscount_RepoThrows_Throws()
        {
            _betsRepoMock.Setup(x => x.GetUserTokens(1))
                .Throws(new Exception());

            Assert.Throws<Exception>(() => _sut.GetUserTokenFeeDiscount(1));
        }
        
        // ─── PlaceUserBet ───
        
        [Fact]
        public void PlaceUserBet_Valid_AddsBetAndUpdatesTokens()
        {
            _betsRepoMock.Setup(x => x.GetUserTokens(1)).Returns(Tokens(500));
            _usersServiceMock.Setup(x => x.GetUserByID(1)).Returns(CreateUser());
            _betsRepoMock.Setup(x => x.GetBetByID(1)).Returns(CreateBet());
            _betsRepoMock.Setup(x => x.GetUserTokens(1)).Returns(Tokens(500));

            _sut.PlaceUserBet(1, 1, 100, BetVote.YES);

            _betsRepoMock.Verify(x => x.AddUserBet(It.IsAny<UsersBets>()), Times.Once);
            _betsRepoMock.Verify(x => x.UpdateUserTokens(1, 400), Times.Once);
        }

        [Fact]
        public void PlaceUserBet_NotEnoughTokens_Throws()
        {
            _betsRepoMock.Setup(x => x.GetUserTokens(1)).Returns(Tokens(10));

            Assert.Throws<Exception>(() => _sut.PlaceUserBet(1, 1, 100, BetVote.YES));
        }
        
        // ─── didUserWinBet ───
        
        [Fact]
        public void didUserWinBet_UserWins_ReturnsTrue()
        {
            var bet = CreateBet();
            var userBet = UserBet(BetVote.YES);

            _betsRepoMock.Setup(x => x.GetUserBetByID(1, 1)).Returns(userBet);
            _betsRepoMock.Setup(x => x.GetBetByID(1)).Returns(bet);

            _postsServiceMock.Setup(x => x.GetPostsByCommunityIDs(It.IsAny<int[]>(), 0, int.MaxValue))
                .Returns(new List<Post> { new() { CreationTime = DateTime.Now, Title = "key" } });

            var result = _sut.didUserWinBet(1, 1);

            Assert.True(result);
        }
        
        // ─── SearchBetsByKeywords ───
        
        [Fact]
        public void SearchBetsByKeywords_ReturnsResults()
        {
            _betsRepoMock.Setup(x => x.GetBetsByKeywords("test"))
                .Returns(new List<Bet> { CreateBet() });

            var result = _sut.SearchBetsByKeywords("test");

            Assert.NotEmpty(result);
        }

        [Fact]
        public void SearchBetsByKeywords_RepoThrows_Throws()
        {
            _betsRepoMock.Setup(x => x.GetBetsByKeywords("test"))
                .Throws(new Exception());

            Assert.Throws<Exception>(() => _sut.SearchBetsByKeywords("test"));
        }
        
        // ─── ValidateCreateBet ───
        
        [Fact]
        public void ValidateCreateBet_NotEnoughTokens_Throws()
        {
            _betsRepoMock.Setup(x => x.GetUserTokens(1)).Returns(Tokens(3));

            Assert.Throws<Exception>(() => _sut.ValidateCreateBet(1, CreateBet(expr: "valid")));
        }

        [Fact]
        public void ValidateCreateBet_ExpressionTooShort_Throws()
        {
            _betsRepoMock.Setup(x => x.GetUserTokens(1)).Returns(Tokens(10));

            Assert.Throws<Exception>(() => _sut.ValidateCreateBet(1, CreateBet(expr: "ab")));
        }

        [Fact]
        public void ValidateCreateBet_ExpressionTooLong_Throws()
        {
            _betsRepoMock.Setup(x => x.GetUserTokens(1)).Returns(Tokens(10));

            Assert.Throws<Exception>(() => _sut.ValidateCreateBet(1, CreateBet(expr: new string('a', 51))));
        }
        
        [Fact]
        public void ValidateCreateBet_InvalidTimeInterval_Throws()
        {
            _betsRepoMock.Setup(x => x.GetUserTokens(1)).Returns(Tokens(10));

            var bet = CreateBet(
                start: DateTime.Now,
                end: DateTime.Now.AddMinutes(-1)
            );

            Assert.Throws<Exception>(() => _sut.ValidateCreateBet(1, bet));
        }
        
        // ─── ValidatePlaceUserBet ───
      
        [Fact]
        public void ValidatePlaceUserBet_AmountTooHigh_Throws()
        {
            _betsRepoMock.Setup(x => x.GetUserTokens(1)).Returns(Tokens(2000));

            Assert.Throws<Exception>(() => _sut.ValidatePlaceUserBet(1, 1500));
        }
    }
}