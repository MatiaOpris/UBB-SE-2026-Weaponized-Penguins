using System;
using System.Collections.Generic;
using System.Text;

namespace Boards_WP.Data.Models;

public class CommentsVotes
{
    public User User { get; init; }
    public Comment Comment { get; init; }
    public VoteType Vote { get; set; } = VoteType.None;

}
