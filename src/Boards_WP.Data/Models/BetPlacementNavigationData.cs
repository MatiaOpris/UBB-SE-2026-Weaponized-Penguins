using System;

namespace Boards_WP.Data.Models
{
    public class BetPlacementNavigationData
    {
        public required Bet SelectedBet { get; set; }
        public BetVote SelectedVote { get; set; }
        public decimal SelectedOdd { get; set; }
    }
}
