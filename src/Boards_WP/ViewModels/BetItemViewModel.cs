using System;
using System.Windows.Input;

using Boards_WP.Data.Models;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Boards_WP.ViewModels
{
    public partial class BetItemViewModel : ObservableObject
    {
        private readonly Action<BetVote, decimal>? _openBetPlacement;

        [ObservableProperty]
        private Bet _betData;

        [ObservableProperty]
        private string _yesOdds;

        [ObservableProperty]
        private string _noOdds;

        public decimal YesOddValue { get; }
        public decimal NoOddValue { get; }

        public ICommand BetYesCommand { get; }
        public ICommand BetNoCommand { get; }

        public string TimeLeft
        {
            get
            {
                var span = BetData.EndingTime - DateTime.Now;
                if (span.TotalSeconds <= 0) return "Expired";

                return span.Days > 0
                    ? $"{span.Days}d {span.Hours:D2}h"
                    : $"{span.Hours:D2}h {span.Minutes:D2}m";
            }
        }

        public BetItemViewModel(Bet bet, decimal yesOdd, decimal noOdd, Action<BetVote, decimal>? openBetPlacement = null)
        {
            BetData = bet;
            YesOddValue = yesOdd;
            NoOddValue = noOdd;
            YesOdds = $"{yesOdd:F2}x";
            NoOdds = $"{noOdd:F2}x";
            _openBetPlacement = openBetPlacement;

            BetYesCommand = new RelayCommand(() => _openBetPlacement?.Invoke(BetVote.YES, YesOddValue));
            BetNoCommand = new RelayCommand(() => _openBetPlacement?.Invoke(BetVote.NO, NoOddValue));
        }
    }
}