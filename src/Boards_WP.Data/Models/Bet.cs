using System;

namespace Boards_WP.Data.Models
{

    public class Bet
    {
        public int BetID { get; init; }
        public required Community BetCommunity { get; init; }
        public BetType Type { get; set; }
        public DateTime StartingTime { get; set; }
        public DateTime EndingTime { get; set; }
        public String Expression { get; set; } = String.Empty;
        public int YesAmount { get; set; }
        public int NoAmount { get; set; }
    }
}
