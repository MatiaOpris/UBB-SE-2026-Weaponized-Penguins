using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System;

using Boards_WP.Data.Models;
using Boards_WP.Data.Services;
using Boards_WP.Data.Services.Interfaces;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.DependencyInjection;

namespace Boards_WP.ViewModels
{
    public partial class BetsViewModel : ObservableObject
    {
        private readonly IBetsService _betsService;
        private readonly UserSession _userSession;
        private readonly INavigationService _navigationService;
        private readonly MainViewModel _mainViewModel;

        private readonly HashSet<int> _claimedBetIDs = new();

        public MainViewModel MainViewModel => _mainViewModel;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HomeTabOpacity))]
        [NotifyPropertyChangedFor(nameof(UserBetsTabOpacity))]
        private bool _isHomeTabSelected = true;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(OngoingSubTabOpacity))]
        [NotifyPropertyChangedFor(nameof(ExpiredSubTabOpacity))]
        private bool _isOngoingSubTabSelected = true;

        public ObservableCollection<BetItemViewModel> FilteredBets { get; } = new();
        public ObservableCollection<UserBetItemViewModel> OngoingUserBets { get; } = new();
        public ObservableCollection<UserBetItemViewModel> ExpiredUserBets { get; } = new();

        public double HomeTabOpacity => IsHomeTabSelected ? 1.0 : 0.6;
        public double UserBetsTabOpacity => IsHomeTabSelected ? 0.6 : 1.0;
        public double OngoingSubTabOpacity => IsOngoingSubTabSelected ? 1.0 : 0.6;
        public double ExpiredSubTabOpacity => IsOngoingSubTabSelected ? 0.6 : 1.0;

        public BetsViewModel(IBetsService betsService, INavigationService navigationService, UserSession userSession)
        {
            _betsService = betsService;
            _navigationService = navigationService;
            _userSession = userSession;

            _mainViewModel = App.GetService<MainViewModel>();

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

            _navigationService.NavigateTo(typeof(Views.Pages.PlaceBetView), payload);
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
            var allBets = _betsService.GetAllBets();

            var currentUserId = _userSession?.CurrentUser?.UserID ?? 1;

            foreach (var bet in allBets)
            {
                var (yesOdd, noOdd) = _betsService.CalculateBetOdds(bet.BetID, currentUserId);

                FilteredBets.Add(new BetItemViewModel(
                    bet, yesOdd, noOdd,
                    (vote, odd) => OpenBetPlacement(bet, vote, odd)));
            }
        }

        private void LoadBetsByKeywords(string keywords)
        {
            FilteredBets.Clear();
            var bets = _betsService.SearchBetsByKeywords(keywords);
            var currentUserId = _userSession?.CurrentUser?.UserID ?? 1;

            foreach (var bet in bets)
            {
                var (yesOdd, noOdd) = _betsService.CalculateBetOdds(bet.BetID, currentUserId);

                FilteredBets.Add(new BetItemViewModel(
                    bet, yesOdd, noOdd,
                    (vote, odd) => OpenBetPlacement(bet, vote, odd)));
            }
        }

        private void LoadCurrentUserBets()
        {
            OngoingUserBets.Clear();
            ExpiredUserBets.Clear();

            var currentUserId = _userSession?.CurrentUser?.UserID ?? 0;
            if (currentUserId == 0) return;

            var ongoingBets = _betsService.GetOngoingPlacedBetsOfUser(currentUserId);
            foreach (var bet in ongoingBets)
                OngoingUserBets.Add(new UserBetItemViewModel(bet, _betsService, currentUserId, _claimedBetIDs, OnPayoutClaimed));

            var expiredBets = _betsService.GetExpiredPlacedBetsOfUser(currentUserId);
            foreach (var bet in expiredBets)
                ExpiredUserBets.Add(new UserBetItemViewModel(bet, _betsService, currentUserId, _claimedBetIDs, OnPayoutClaimed));
        }

        public ICommand CreateBetCommand { get; set; }
    }
}