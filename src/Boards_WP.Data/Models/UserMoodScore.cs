using System;
using System.Collections.Generic;
using System.Text;

namespace Boards_WP.Data.Models;

public class UserMoodScore
{
    public User User { get; set; }
    public Category Category { get; set; }
    public int Score { get; set; }

}
