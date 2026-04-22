using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Boards_WP.ViewModels
{
    /// <summary>
    /// Represents the view model for browsing and managing bets.
    /// </summary>
    public partial class BetsViewModel : ObservableObject
    {
        private readonly IBetsService betsService;
        private readonly UserSession userSession;
        private readonly INavigationService navigationService;
        private readonly MainViewModel mainViewModel;

        private readonly HashSet<int> claimedBetIDs = new ();

        public MainViewModel MainViewModel => mainViewModel;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HomeTabOpacity))]
        [NotifyPropertyChangedFor(nameof(UserBetsTabOpacity))]
        private bool isHomeTabSelected = true;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(OngoingSubTabOpacity))]
        [NotifyPropertyChangedFor(nameof(ExpiredSubTabOpacity))]
        private bool isOngoingSubTabSelected = true;

        public ObservableCollection<BetItemViewModel> FilteredBets { get; } = new ();
        public ObservableCollection<UserBetItemViewModel> OngoingUserBets { get; } = new ();
        public ObservableCollection<UserBetItemViewModel> ExpiredUserBets { get; } = new ();

        public double HomeTabOpacity => IsHomeTabSelected ? 1.0 : 0.6;
        public double UserBetsTabOpacity => IsHomeTabSelected ? 0.6 : 1.0;
        public double OngoingSubTabOpacity => IsOngoingSubTabSelected ? 1.0 : 0.6;
        public double ExpiredSubTabOpacity => IsOngoingSubTabSelected ? 0.6 : 1.0;

        public BetsViewModel(IBetsService betsService, INavigationService navigationService, UserSession userSession)
        {
            this.betsService = betsService;
            this.navigationService = navigationService;
            this.userSession = userSession;

            mainViewModel = App.GetService<MainViewModel>();

            LoadHomeBets();
        }

        public void Initialize(string? keywords)
        {
            if (string.IsNullOrWhiteSpace(keywords))
            {
                ShowHome();
                return;
            }

            IsHomeTabSelected = true;
            LoadBetsByKeywords(keywords);
        }

        private void OnPayoutClaimed(int updatedTokens)
        {
            TokenEvents.NotifyTokensUpdated(updatedTokens);
        }

        private void OpenBetPlacement(Bet bet, BetVote vote, decimal odd)
        {
            var payload = new BetPlacementNavigationData
            {
                SelectedBet = bet,
                SelectedVote = vote,
                SelectedOdd = odd
            };

            navigationService.NavigateTo(typeof(Views.Pages.PlaceBetView), payload);
        }

        [RelayCommand]
        private void ShowHome()
        {
            IsHomeTabSelected = true;
            LoadHomeBets();
        }

        [RelayCommand]
        private void ShowUserBets()
        {
            IsHomeTabSelected = false;
            IsOngoingSubTabSelected = true;
            LoadCurrentUserBets();
        }

        [RelayCommand]
        private void ShowOngoingSubTab()
        {
            IsOngoingSubTabSelected = true;
        }

        [RelayCommand]
        private void ShowExpiredSubTab()
        {
            IsOngoingSubTabSelected = false;
        }

        private void LoadHomeBets()
        {
            FilteredBets.Clear();
            var allBets = betsService.GetAllBets();

            var currentUserId = userSession?.CurrentUser?.UserID ?? 1;

            foreach (var bet in allBets)
            {
                var (yesOdd, noOdd) = betsService.CalculateBetOdds(bet.BetID, currentUserId);

                FilteredBets.Add(new BetItemViewModel(
                    bet, yesOdd, noOdd,
                    (vote, odd) => OpenBetPlacement(bet, vote, odd)));
            }
        }

        private void LoadBetsByKeywords(string keywords)
        {
            FilteredBets.Clear();
            var bets = betsService.SearchBetsByKeywords(keywords);
            var currentUserId = userSession?.CurrentUser?.UserID ?? 1;

            foreach (var bet in bets)
            {
                var (yesOdd, noOdd) = betsService.CalculateBetOdds(bet.BetID, currentUserId);

                FilteredBets.Add(new BetItemViewModel(
                    bet, yesOdd, noOdd,
                    (vote, odd) => OpenBetPlacement(bet, vote, odd)));
            }
        }

        private void LoadCurrentUserBets()
        {
            OngoingUserBets.Clear();
            ExpiredUserBets.Clear();

            var currentUserId = userSession?.CurrentUser?.UserID ?? 0;
            if (currentUserId == 0)
            {
                return;
            }

            var ongoingBets = betsService.GetOngoingPlacedBetsOfUser(currentUserId);
            foreach (var bet in ongoingBets)
            {
                OngoingUserBets.Add(new UserBetItemViewModel(bet, betsService, currentUserId, claimedBetIDs, OnPayoutClaimed));
            }

            var expiredBets = betsService.GetExpiredPlacedBetsOfUser(currentUserId);
            foreach (var bet in expiredBets)
            {
                ExpiredUserBets.Add(new UserBetItemViewModel(bet, betsService, currentUserId, claimedBetIDs, OnPayoutClaimed));
            }
        }

        public ICommand CreateBetCommand { get; set; }
    }
}
