using System;

using Boards_WP.Data.Models;
using Boards_WP.Data.Services;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Boards_WP.ViewModels
{
    public partial class PlaceBetViewModel : ObservableObject
    {
        private readonly IBetsService _betsService;
        private readonly UserSession _userSession;

        [ObservableProperty]
        private Bet _selectedBet;

        [ObservableProperty]
        private BetVote _selectedVote;

        [ObservableProperty]
        private decimal _selectedOdd;

        public string SelectedOddText => SelectedOdd.ToString("F2");

        [ObservableProperty]
        private int _betAmount;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private string _successMessage = string.Empty;

        public string PotentialWin => Math.Ceiling(BetAmount * SelectedOdd).ToString("0");

        public PlaceBetViewModel(IBetsService betsService, UserSession userSession)
        {
            _betsService = betsService;
            _userSession = userSession;
            _selectedBet = null!;
        }

        public void Initialize(BetPlacementNavigationData payload)
        {
            SelectedBet = payload.SelectedBet;
            SelectedVote = payload.SelectedVote;
            SelectedOdd = payload.SelectedOdd;
            BetAmount = 0;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            OnPropertyChanged(nameof(PotentialWin));
            OnPropertyChanged(nameof(SelectedOddText));
        }

        partial void OnBetAmountChanged(int value)
        {
            OnPropertyChanged(nameof(PotentialWin));
        }

        partial void OnSelectedOddChanged(decimal value)
        {
            OnPropertyChanged(nameof(SelectedOddText));
            OnPropertyChanged(nameof(PotentialWin));
        }

        [RelayCommand]
        private void PlaceBet()
        {
            try
            {
                var userId = _userSession?.CurrentUser?.UserID ?? 0;
                if (userId == 0)
                {
                    ErrorMessage = "No current user available.";
                    SuccessMessage = string.Empty;
                    return;
                }

                _betsService.ValidatePlaceUserBet(userId, BetAmount);
                _betsService.PlaceUserBet(userId, SelectedBet.BetID, BetAmount, SelectedVote);

                ErrorMessage = string.Empty;
                SuccessMessage = "May the luck be with you!";

                BetPlaced?.Invoke();
            }
            catch (Exception ex)
            {
                SuccessMessage = string.Empty;
                ErrorMessage = ex.Message;
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            BetCancelled?.Invoke();
        }

        public event Action? BetPlaced;
        public event Action? BetCancelled;
    }
}
