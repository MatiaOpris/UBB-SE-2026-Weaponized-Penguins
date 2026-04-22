using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Boards_WP.ViewModels
{
    public partial class PlaceBetViewModel : ObservableObject
    {
        private readonly IBetsService betsService;
        private readonly UserSession userSession;

        [ObservableProperty]
        private Bet selectedBet;

        [ObservableProperty]
        private BetVote selectedVote;

        [ObservableProperty]
        private decimal selectedOdd;

        public string SelectedOddText => SelectedOdd.ToString("F2");

        [ObservableProperty]
        private int betAmount;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private string successMessage = string.Empty;

        public string PotentialWin => Math.Ceiling(BetAmount * SelectedOdd).ToString("0");

        public PlaceBetViewModel(IBetsService betsService, UserSession userSession)
        {
            this.betsService = betsService;
            this.userSession = userSession;
            selectedBet = null!;
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
                var userId = userSession?.CurrentUser?.UserID ?? 0;
                if (userId == 0)
                {
                    ErrorMessage = "No current user available.";
                    SuccessMessage = string.Empty;
                    return;
                }

                betsService.ValidatePlaceUserBet(userId, BetAmount);
                betsService.PlaceUserBet(userId, SelectedBet.BetID, BetAmount, SelectedVote);

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