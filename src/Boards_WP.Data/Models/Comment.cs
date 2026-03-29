using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;

namespace Boards_WP.Data.Models
{
    public class Comment
    {
        public int CommentID { get; init; }
        public Post Post { get; init; }
        public Comment? ParentComment { get; init; }
        public User Owner { get; init; }
        public String Description { get; set; } = String.Empty;
        public int Score { get; set; }
        public DateTime CreationTime { get; set; }
        public int Indentation { get; init; }
        public Boolean IsDeleted { get; set; }
        public VoteType UserCurrentVote { get; set; } = VoteType.None;
        public String GetShareLink => $"boards://post/{Post.PostID}";

    }
}
