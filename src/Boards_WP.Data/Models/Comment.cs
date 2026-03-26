using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;

namespace Boards_WP.Data.Models
{
    public class Comment
    {
        private int commentID { get; set; }
        private int postID { get; set; }
        private int? parentID { get; set; }
        private String description { get; set; } = String.Empty;
        private int score { get; set; }
        private DateTime creationTime { get; set; }
        private int indentation { get; set; }
        private Boolean isDeleted { get; set; }
        private String ownerName { get; set; } = String.Empty;
        private byte[]? ownerPicture { get; set; }
        private VoteType userCurrentVote { get; set; } = VoteType.None;

        public String getShareLink => $"boards://post/{postID}";

    }
}
