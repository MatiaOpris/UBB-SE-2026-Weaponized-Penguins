using System;
using System.Collections.Generic;
using System.Text;

namespace Boards_WP.Data.Models;

public class CommentsVotes
{
    private int userID { get; set; }
    private int commentID { get; set; }
    private VoteType vote { get; set; } = VoteType.None;

}
