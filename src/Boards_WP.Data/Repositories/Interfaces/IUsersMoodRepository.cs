using System;
using System.Collections.Generic;
using System.Text;

namespace Boards_WP.Data.Repositories.Interfaces;

public interface IUsersMoodRepository
{
    public Dictionary<int, int> GetUsersMoodScores(int userID, int categoryCount);

    public void UpdateUsersMoodScores(int userID, Dictionary<int, int> newMoodScores);
}
