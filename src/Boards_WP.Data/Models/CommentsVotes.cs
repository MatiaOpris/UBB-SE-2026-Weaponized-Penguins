using System;
using System.Collections.Generic;
using System.Text;

namespace Boards_WP.Data.Models;

public class CommentsVotes
{
    public required User User { get; init; }
    public required Comment Comment { get; init; }
    public VoteType Vote { get; set; } = VoteType.None;

}
