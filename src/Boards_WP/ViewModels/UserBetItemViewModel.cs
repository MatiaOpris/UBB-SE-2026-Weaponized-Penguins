using System;
using System.Windows.Input;

using Boards_WP.Data.Models;
using Boards_WP.Data.Services;

using CommunityToolkit.Mvvm.Input;

namespace Boards_WP.ViewModels
{
    public class UserBetItemViewModel
    {
        public Bet BetData { get; }
        public int BetAmount { get; }
        public BetVote Vote { get; }
        public decimal LockedOdd { get; }
        public bool IsExpired { get; }
        public bool HasWon { get; }

        public string LockedOddText => $"{LockedOdd:F2}x";
        public string PotentialWinText => $"{Math.Ceiling(BetAmount * LockedOdd):0}";

        public string ResultText => HasWon ? "WON" : "LOST";
        public Windows.UI.Color ResultColor => HasWon
            ? Windows.UI.Color.FromArgb(255, 46, 125, 50)
            : Windows.UI.Color.FromArgb(255, 198, 40, 40);
        public Windows.UI.Color ResultBackground => HasWon
            ? Windows.UI.Color.FromArgb(255, 232, 245, 233)
            : Windows.UI.Color.FromArgb(255, 255, 235, 238);
        public Microsoft.UI.Xaml.Visibility ClaimButtonVisibility =>
            HasWon ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;

        public bool CanClaimPayout { get; private set; }

        public ICommand ClaimPayoutCommand { get; }

        public UserBetItemViewModel(UsersBets userBet, IBetsService betsService, int currentUserID, HashSet<int> claimedBetIDs, Action<int> onTokensUpdated)
        {
            BetData = userBet.SelectedBet;
            BetAmount = userBet.Amount;
            Vote = userBet.Vote;
            LockedOdd = userBet.Odd;
            IsExpired = BetData.EndingTime < DateTime.Now;

            if (IsExpired)
            {
                HasWon = betsService.didUserWinBet(currentUserID, BetData.BetID);
                CanClaimPayout = HasWon && !claimedBetIDs.Contains(BetData.BetID);
            }

            ClaimPayoutCommand = new RelayCommand(() =>
            {
                if (!CanClaimPayout) return;

                betsService.ExecuteActionsByBetResult(currentUserID, BetData.BetID);
                claimedBetIDs.Add(BetData.BetID);
                CanClaimPayout = false;

                int updatedTokens = betsService.GetUserTokenCount(currentUserID);
                onTokensUpdated?.Invoke(updatedTokens);
            },
            () => CanClaimPayout);
        }
    }
}