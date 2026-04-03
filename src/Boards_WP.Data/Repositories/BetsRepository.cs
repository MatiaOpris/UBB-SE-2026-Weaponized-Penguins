using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Boards_WP.Data.Repositories;

public class BetsRepository : IBetsRepository
{
    private readonly String _connectionString;

    public BetsRepository(String connectionString)
    {
        _connectionString = connectionString;
    }

    public int AddBet(Bet b)
    {
        const string query = @"
                INSERT INTO Bets (communityID, betType, startingTime, endingTime, expression, yesAmount, notAmount)
                OUTPUT INSERTED.betID
                VALUES (@communityID, @betType, @startingTime, @endingTime, @expression, @yesAmount, @notAmount)";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(query, connection);

        command.Parameters.Add("@communityID", SqlDbType.Int).Value = b.BetCommunity.CommunityID;
        command.Parameters.Add("@betType", SqlDbType.Int).Value = b.Type;
        command.Parameters.Add("@startingTime", SqlDbType.DateTime).Value = b.StartingTime;
        command.Parameters.Add("@endingTime", SqlDbType.DateTime).Value = b.EndingTime;
        command.Parameters.Add("@expression", SqlDbType.NVarChar).Value = b.Expression;
        command.Parameters.Add("@yesAmount", SqlDbType.Int).Value = b.YesAmount;
        command.Parameters.Add("@notAmount", SqlDbType.Int).Value = b.NoAmount;

        connection.Open();
        return (int)command.ExecuteScalar()!;
    }

    public void AddUserBet(UsersBets UserBet)
    {
        const string query = @"
                INSERT INTO UsersBets (userID, betID, amount, odd, betVote)
                VALUES (@userID, @betID, @amount, @odd, @betVote)";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(query, connection);

        command.Parameters.Add("@userID", SqlDbType.Int).Value = UserBet.BettingUser.UserID;
        command.Parameters.Add("@betID", SqlDbType.Int).Value = UserBet.SelectedBet.BetID;
        command.Parameters.Add("@amount", SqlDbType.Int).Value = UserBet.Amount;
        command.Parameters.Add("@odd", SqlDbType.Decimal).Value = UserBet.Odd;
        command.Parameters.Add("@betVote", SqlDbType.Int).Value = UserBet.Vote;

        connection.Open();
        command.ExecuteNonQuery();
    }

    public void AddUserTokens(UsersTokens userToken)
    {
        const string query = @"
                INSERT INTO UsersTokens (userID, tokens, lastSeen)
                VALUES (@userID, @tokens, @lastSeen)";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(query, connection);

        command.Parameters.Add("@userID", SqlDbType.Int).Value = userToken.CurrentUser.UserID;
        command.Parameters.Add("@tokens", SqlDbType.Int).Value = userToken.TokensNumber;
        command.Parameters.Add("@lastSeen", SqlDbType.DateTime).Value = userToken.LastSeen;

        connection.Open();
        command.ExecuteNonQuery();
    }

    public List<Bet> GetAllBetsSortedByDate()
    {
        var bets = new List<Bet>();

        const string query = @"
    SELECT 
        b.betID, b.communityID, b.betType, b.startingTime, b.endingTime, b.expression, b.yesAmount, b.notAmount,
        c.communityID AS communityID, c.name AS communityName, c.description, c.picture, c.banner, c.membersNumber,
        u.userID AS adminUserID, u.username AS adminUsername, u.email AS adminEmail,
        u.passwordHash AS adminPasswordHash, u.avatarUrl AS adminAvatarUrl, u.bio AS adminBio, u.status AS adminStatus
    FROM Bets b
    INNER JOIN Communities c ON b.communityID = c.communityID
    INNER JOIN Users u ON c.adminID = u.userID
    ORDER BY b.startingTime DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(query, connection);
        connection.Open();

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            var admin = new User
            {
                UserID = (int)reader["adminUserID"],
                Username = reader["adminUsername"].ToString(),
                Email = reader["adminEmail"].ToString(),
                PasswordHash = reader["adminPasswordHash"].ToString(),
                AvatarUrl = reader["adminAvatarUrl"].ToString(),
                Bio = reader["adminBio"].ToString(),
                Status = reader["adminStatus"].ToString()
            };

            var community = new Community
            {
                CommunityID = (int)reader["communityID"],
                Name = reader["communityName"].ToString(),
                Description = reader["description"].ToString(),
                Picture = reader["picture"] as byte[],
                Banner = reader["banner"] as byte[],
                MembersNumber = (int)reader["membersNumber"],
                Admin = admin
            };

            var bet = new Bet
            {
                BetID = (int)reader["betID"],
                BetCommunity = community,
                Type = (BetType)(int)reader["betType"], // enum cast
                StartingTime = (DateTime)reader["startingTime"],
                EndingTime = (DateTime)reader["endingTime"],
                Expression = reader["expression"].ToString(),
                YesAmount = (int)reader["yesAmount"],
                NoAmount = (int)reader["notAmount"]
            };

            bets.Add(bet);
        }

        return bets;
    }

    public Bet GetBetByID(int BetID)
    {
        const string query = @"
    SELECT 
        b.betID, b.communityID, b.betType, b.startingTime, b.endingTime, b.expression, b.yesAmount, b.notAmount,
        c.communityID AS communityID, c.name AS communityName, c.description, c.picture, c.banner, c.membersNumber,
        u.userID AS adminUserID, u.username AS adminUsername, u.email AS adminEmail,
        u.passwordHash AS adminPasswordHash, u.avatarUrl AS adminAvatarUrl, u.bio AS adminBio, u.status AS adminStatus
    FROM Bets b
    INNER JOIN Communities c ON b.communityID = c.communityID
    INNER JOIN Users u ON c.adminID = u.userID
    WHERE b.betID = @betID";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(query, connection);
        command.Parameters.Add("@betID", SqlDbType.Int).Value = BetID;

        connection.Open();

        using var reader = command.ExecuteReader();

        if (reader.Read())
        {
            var admin = new User
            {
                UserID = (int)reader["adminUserID"],
                Username = reader["adminUsername"].ToString(),
                Email = reader["adminEmail"].ToString(),
                PasswordHash = reader["adminPasswordHash"].ToString(),
                AvatarUrl = reader["adminAvatarUrl"].ToString(),
                Bio = reader["adminBio"].ToString(),
                Status = reader["adminStatus"].ToString()
            };

            var community = new Community
            {
                CommunityID = (int)reader["communityID"],
                Name = reader["communityName"].ToString(),
                Description = reader["description"].ToString(),
                Picture = reader["picture"] as byte[],
                Banner = reader["banner"] as byte[],
                MembersNumber = (int)reader["membersNumber"],
                Admin = admin
            };

            return new Bet
            {
                BetID = (int)reader["betID"],
                BetCommunity = community,
                Type = (BetType)(int)reader["betType"],
                StartingTime = (DateTime)reader["startingTime"],
                EndingTime = (DateTime)reader["endingTime"],
                Expression = reader["expression"].ToString(),
                YesAmount = (int)reader["yesAmount"],
                NoAmount = (int)reader["notAmount"]
            };
        }

        return null!;
    }

    public List<Bet> GetBetsByKeywords(string Keywords)
    {
        var bets = new List<Bet>();

        if(Keywords == String.Empty)
            return GetAllBetsSortedByDate(); //if the user doesn't input any keyword, we return all the bets.

        const string query = @"
    SELECT 
        b.betID, b.communityID, b.betType, b.startingTime, b.endingTime, b.expression, b.yesAmount, b.notAmount,
        c.communityID AS communityID, c.name AS communityName, c.description, c.picture, c.banner, c.membersNumber,
        u.userID AS adminUserID, u.username AS adminUsername, u.email AS adminEmail,
        u.passwordHash AS adminPasswordHash, u.avatarUrl AS adminAvatarUrl, u.bio AS adminBio, u.status AS adminStatus
    FROM Bets b
    INNER JOIN Communities c ON b.communityID = c.communityID
    INNER JOIN Users u ON c.adminID = u.userID
    WHERE b.expression LIKE @keywords";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(query, connection);
        command.Parameters.Add("@keywords", SqlDbType.NVarChar).Value = $"%{Keywords}%";

        connection.Open();

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            var admin = new User
            {
                UserID = (int)reader["adminUserID"],
                Username = reader["adminUsername"].ToString(),
                Email = reader["adminEmail"].ToString(),
                PasswordHash = reader["adminPasswordHash"].ToString(),
                AvatarUrl = reader["adminAvatarUrl"].ToString(),
                Bio = reader["adminBio"].ToString(),
                Status = reader["adminStatus"].ToString()
            };

            var community = new Community
            {
                CommunityID = (int)reader["communityID"],
                Name = reader["communityName"].ToString(),
                Description = reader["description"].ToString(),
                Picture = reader["picture"] as byte[],
                Banner = reader["banner"] as byte[],
                MembersNumber = (int)reader["membersNumber"],
                Admin = admin
            };

            bets.Add(new Bet
            {
                BetID = (int)reader["betID"],
                BetCommunity = community,
                Type = (BetType)(int)reader["betType"],
                StartingTime = (DateTime)reader["startingTime"],
                EndingTime = (DateTime)reader["endingTime"],
                Expression = reader["expression"].ToString(),
                YesAmount = (int)reader["yesAmount"],
                NoAmount = (int)reader["notAmount"]
            });
        }

        return bets;
    }

    public UsersBets GetUserBetByID(int UserID, int BetID)
    {
        const string query = @"
    SELECT 
        ub.amount, ub.odd, ub.betVote,

        bu.userID AS betUserID, bu.username AS betUsername, bu.email AS betEmail,
        bu.passwordHash AS betPasswordHash, bu.avatarUrl AS betAvatarUrl, bu.bio AS betBio, bu.status AS betStatus,

        b.betID, b.betType, b.startingTime, b.endingTime, b.expression, b.yesAmount, b.notAmount,

        c.communityID, c.name AS communityName, c.description, c.picture, c.banner, c.membersNumber,

        au.userID AS adminUserID, au.username AS adminUsername, au.email AS adminEmail,
        au.passwordHash AS adminPasswordHash, au.avatarUrl AS adminAvatarUrl, au.bio AS adminBio, au.status AS adminStatus

    FROM UsersBets ub
    INNER JOIN Users bu ON ub.userID = bu.userID
    INNER JOIN Bets b ON ub.betID = b.betID
    INNER JOIN Communities c ON b.communityID = c.communityID
    INNER JOIN Users au ON c.adminID = au.userID

    WHERE ub.userID = @userID AND ub.betID = @betID";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(query, connection);

        command.Parameters.Add("@userID", SqlDbType.Int).Value = UserID;
        command.Parameters.Add("@betID", SqlDbType.Int).Value = BetID;

        connection.Open();

        using var reader = command.ExecuteReader();

        if (reader.Read())
        {
            var admin = new User
            {
                UserID = (int)reader["adminUserID"],
                Username = reader["adminUsername"].ToString(),
                Email = reader["adminEmail"].ToString(),
                PasswordHash = reader["adminPasswordHash"].ToString(),
                AvatarUrl = reader["adminAvatarUrl"].ToString(),
                Bio = reader["adminBio"].ToString(),
                Status = reader["adminStatus"].ToString()
            };

            var community = new Community
            {
                CommunityID = (int)reader["communityID"],
                Name = reader["communityName"].ToString(),
                Description = reader["description"].ToString(),
                Picture = reader["picture"] as byte[],
                Banner = reader["banner"] as byte[],
                MembersNumber = (int)reader["membersNumber"],
                Admin = admin
            };

            var bet = new Bet
            {
                BetID = (int)reader["betID"],
                BetCommunity = community,
                Type = (BetType)(int)reader["betType"],
                StartingTime = (DateTime)reader["startingTime"],
                EndingTime = (DateTime)reader["endingTime"],
                Expression = reader["expression"].ToString(),
                YesAmount = (int)reader["yesAmount"],
                NoAmount = (int)reader["notAmount"]
            };

            var user = new User
            {
                UserID = (int)reader["betUserID"],
                Username = reader["betUsername"].ToString(),
                Email = reader["betEmail"].ToString(),
                PasswordHash = reader["betPasswordHash"].ToString(),
                AvatarUrl = reader["betAvatarUrl"].ToString(),
                Bio = reader["betBio"].ToString(),
                Status = reader["betStatus"].ToString()
            };

            return new UsersBets
            {
                BettingUser = user,
                SelectedBet = bet,
                Amount = (int)reader["amount"],
                Odd = (decimal)reader["odd"], 
                Vote = (BetVote)(int)reader["betVote"]
            };
        }

        return null!;
    }

    public List<UsersBets> GetUserBetsByBet(int BetID)
    {
        var list = new List<UsersBets>();

        const string query = @"
    SELECT
        ub.amount, ub.odd, ub.betVote,

        bu.userID AS betUserID, bu.username AS betUsername, bu.email AS betEmail,
        bu.passwordHash AS betPasswordHash, bu.avatarUrl AS betAvatarUrl, bu.bio AS betBio, bu.status AS betStatus,

        b.betID, b.betType, b.startingTime, b.endingTime, b.expression, b.yesAmount, b.notAmount,

        c.communityID, c.name AS communityName, c.description, c.picture, c.banner, c.membersNumber,

      
        au.userID AS adminUserID, au.username AS adminUsername, au.email AS adminEmail,
        au.passwordHash AS adminPasswordHash, au.avatarUrl AS adminAvatarUrl, au.bio AS adminBio, au.status AS adminStatus

    FROM UsersBets ub
    INNER JOIN Users bu ON ub.userID = bu.userID
    INNER JOIN Bets b ON ub.betID = b.betID
    INNER JOIN Communities c ON b.communityID = c.communityID
    INNER JOIN Users au ON c.adminID = au.userID

    WHERE ub.betID = @betID";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(query, connection);

        command.Parameters.Add("@betID", SqlDbType.Int).Value = BetID;

        connection.Open();

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            var admin = new User
            {
                UserID = (int)reader["adminUserID"],
                Username = reader["adminUsername"].ToString(),
                Email = reader["adminEmail"].ToString(),
                PasswordHash = reader["adminPasswordHash"].ToString(),
                AvatarUrl = reader["adminAvatarUrl"].ToString(),
                Bio = reader["adminBio"].ToString(),
                Status = reader["adminStatus"].ToString()
            };

            var community = new Community
            {
                CommunityID = (int)reader["communityID"],
                Name = reader["communityName"].ToString(),
                Description = reader["description"].ToString(),
                Picture = reader["picture"] as byte[],
                Banner = reader["banner"] as byte[],
                MembersNumber = (int)reader["membersNumber"],
                Admin = admin
            };

            var bet = new Bet
            {
                BetID = (int)reader["betID"],
                BetCommunity = community,
                Type = (BetType)(int)reader["betType"],
                StartingTime = (DateTime)reader["startingTime"],
                EndingTime = (DateTime)reader["endingTime"],
                Expression = reader["expression"].ToString(),
                YesAmount = (int)reader["yesAmount"],
                NoAmount = (int)reader["notAmount"]
            };

            var user = new User
            {
                UserID = (int)reader["betUserID"],
                Username = reader["betUsername"].ToString(),
                Email = reader["betEmail"].ToString(),
                PasswordHash = reader["betPasswordHash"].ToString(),
                AvatarUrl = reader["betAvatarUrl"].ToString(),
                Bio = reader["betBio"].ToString(),
                Status = reader["betStatus"].ToString()
            };

            list.Add(new UsersBets
            {
                BettingUser = user,
                SelectedBet = bet,
                Amount = (int)reader["amount"],
                Odd = (decimal)reader["odd"],
                Vote = (BetVote)(int)reader["betVote"]
            });
        }

        return list;
    }

    public List<UsersBets> GetUserBetsByUser(int UserID)
    {
        var list = new List<UsersBets>();

        const string query = @"
    SELECT
        ub.amount, ub.odd, ub.betVote,

        bu.userID AS betUserID, bu.username AS betUsername, bu.email AS betEmail,
        bu.passwordHash AS betPasswordHash, bu.avatarUrl AS betAvatarUrl, bu.bio AS betBio, bu.status AS betStatus,

        b.betID, b.betType, b.startingTime, b.endingTime, b.expression, b.yesAmount, b.notAmount,

        c.communityID, c.name AS communityName, c.description, c.picture, c.banner, c.membersNumber,

        au.userID AS adminUserID, au.username AS adminUsername, au.email AS adminEmail,
        au.passwordHash AS adminPasswordHash, au.avatarUrl AS adminAvatarUrl, au.bio AS adminBio, au.status AS adminStatus

    FROM UsersBets ub
    INNER JOIN Users bu ON ub.userID = bu.userID
    INNER JOIN Bets b ON ub.betID = b.betID
    INNER JOIN Communities c ON b.communityID = c.communityID
    INNER JOIN Users au ON c.adminID = au.userID

    WHERE ub.userID = @userID";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(query, connection);

        command.Parameters.Add("@userID", SqlDbType.Int).Value = UserID;

        connection.Open();

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            var admin = new User
            {
                UserID = (int)reader["adminUserID"],
                Username = reader["adminUsername"].ToString(),
                Email = reader["adminEmail"].ToString(),
                PasswordHash = reader["adminPasswordHash"].ToString(),
                AvatarUrl = reader["adminAvatarUrl"].ToString(),
                Bio = reader["adminBio"].ToString(),
                Status = reader["adminStatus"].ToString()
            };

            var community = new Community
            {
                CommunityID = (int)reader["communityID"],
                Name = reader["communityName"].ToString(),
                Description = reader["description"].ToString(),
                Picture = reader["picture"] as byte[],
                Banner = reader["banner"] as byte[],
                MembersNumber = (int)reader["membersNumber"],
                Admin = admin
            };

            var bet = new Bet
            {
                BetID = (int)reader["betID"],
                BetCommunity = community,
                Type = (BetType)(int)reader["betType"],
                StartingTime = (DateTime)reader["startingTime"],
                EndingTime = (DateTime)reader["endingTime"],
                Expression = reader["expression"].ToString(),
                YesAmount = (int)reader["yesAmount"],
                NoAmount = (int)reader["notAmount"]
            };

            var user = new User
            {
                UserID = (int)reader["betUserID"],
                Username = reader["betUsername"].ToString(),
                Email = reader["betEmail"].ToString(),
                PasswordHash = reader["betPasswordHash"].ToString(),
                AvatarUrl = reader["betAvatarUrl"].ToString(),
                Bio = reader["betBio"].ToString(),
                Status = reader["betStatus"].ToString()
            };

            list.Add(new UsersBets
            {
                BettingUser = user,
                SelectedBet = bet,
                Amount = (int)reader["amount"],
                Odd = (decimal)reader["odd"],
                Vote = (BetVote)(int)reader["betVote"]
            });
        }

        return list;
    }

    public UsersTokens GetUserTokens(int UserID)
    {
        const string query = @"
    SELECT 
        ut.tokens, ut.lastSeen,

        u.userID, u.username, u.email,
        u.passwordHash, u.avatarUrl, u.bio, u.status

    FROM UsersTokens ut
    INNER JOIN Users u ON ut.userID = u.userID
    WHERE ut.userID = @userID";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(query, connection);

        command.Parameters.Add("@userID", SqlDbType.Int).Value = UserID;

        connection.Open();

        using var reader = command.ExecuteReader();

        if (reader.Read())
        {
            var user = new User
            {
                UserID = (int)reader["userID"],
                Username = reader["username"].ToString(),
                Email = reader["email"].ToString(),
                PasswordHash = reader["passwordHash"].ToString(),
                AvatarUrl = reader["avatarUrl"].ToString(),
                Bio = reader["bio"].ToString(),
                Status = reader["status"].ToString()
            };

            return new UsersTokens
            {
                CurrentUser = user,
                TokensNumber = (int)reader["tokens"],
                LastSeen = (DateTime)reader["lastSeen"]
            };
        }

        return null!;
    }

    public void RemoveBet(int BetID)
    {
        const string query = @"DELETE FROM Bets WHERE betID = @betID";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(query, connection);

        command.Parameters.Add("@betID", SqlDbType.Int).Value = BetID;

        connection.Open();
        command.ExecuteNonQuery();
    }

    public void UpdateBetAmounts(int BetID, int YesAmount, int NoAmount)
    {
        const string query = @"
        UPDATE Bets 
        SET yesAmount = @yes, notAmount = @no 
        WHERE betID = @betID";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(query, connection);

        command.Parameters.Add("@yes", SqlDbType.Int).Value = YesAmount;
        command.Parameters.Add("@no", SqlDbType.Int).Value = NoAmount;
        command.Parameters.Add("@betID", SqlDbType.Int).Value = BetID;

        connection.Open();
        command.ExecuteNonQuery();
    }

    public void UpdateUserTokens(int UserID, int NewAmount)
    {
        const string query = @"
        UPDATE UsersTokens 
        SET tokens = @tokens, lastSeen = @lastSeen 
        WHERE userID = @userID";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(query, connection);

        command.Parameters.Add("@tokens", SqlDbType.Int).Value = NewAmount;
        command.Parameters.Add("@lastSeen", SqlDbType.DateTime).Value = DateTime.Now;
        command.Parameters.Add("@userID", SqlDbType.Int).Value = UserID;

        connection.Open();
        command.ExecuteNonQuery();
    }

    public bool UserTokensExist(int UserID)
    {
        const string query = @"SELECT COUNT(*) FROM UsersTokens WHERE userID = @userID";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(query, connection);

        command.Parameters.Add("@userID", SqlDbType.Int).Value = UserID;

        connection.Open();

        return (int)command.ExecuteScalar() != 0;
    }
}
