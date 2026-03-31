namespace Boards_WP.Data.Repositories;

public class UsersMoodRepository : IUsersMoodRepository
{
    private readonly string _connectionString;
    private readonly int _interestUnits = 10000;

    public UsersMoodRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public Dictionary<int, int> GetUsersMoodScores(int userID, int categoryCount)
    {
        var scores = new Dictionary<int, int>();
        using var connection = new SqlConnection(_connectionString);

        const string query = "" +
            "SELECT categoryID, score " +
            "FROM UsersMoodScores WHERE userID = @UID";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@UID", userID);

        connection.Open();
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            scores.Add(reader.GetInt32(0), reader.GetInt32(1));
        }

        //new user that has no scores yet
        if (scores.Count == 0) return GetDefaultDistribution(categoryCount);

        return scores;
    }

    public void UpdateUsersMoodScores(int userID, Dictionary<int, int> newMoodScores)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            foreach (var item in newMoodScores)
            {
                const string query = @"
                    UPDATE UsersMoodScores 
                    SET score = @Score 
                    WHERE userID = @UID AND categoryID = @CID";

                using var command = new SqlCommand(query, connection, transaction);
                command.Parameters.AddWithValue("@Score", item.Value);
                command.Parameters.AddWithValue("@UID", userID);
                command.Parameters.AddWithValue("@CID", item.Key);
                command.ExecuteNonQuery();
            }
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    internal Dictionary<int, int> GetDefaultDistribution(int categoryCount)
    {
        var defaults = new Dictionary<int, int>();
        int baseScore = _interestUnits / categoryCount;
        int leftover = _interestUnits % categoryCount;

        for (int i = 1; i <= categoryCount; i++)
        {
            defaults.Add(i, (leftover > 0) ? baseScore + 1 : baseScore);
            if(leftover > 0) leftover--;
        }
        return defaults;
    }
}