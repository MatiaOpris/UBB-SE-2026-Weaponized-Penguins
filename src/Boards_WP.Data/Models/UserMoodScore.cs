using System;
using System.Collections.Generic;
using System.Text;

namespace Boards_WP.Data.Models;

public class UserMoodScore
{
    public required User User { get; set; }
    public required Category Category { get; set; }
    public required int Score { get; set; }

}
