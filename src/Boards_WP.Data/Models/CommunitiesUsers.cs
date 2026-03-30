using System;

namespace Boards_WP.Data.Models
{
    public class CommunitiesUsers
    {
        public required Community JoinedCommunity { get; set; }
        public required User Memeber { get; set; }
    }
}
