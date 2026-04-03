using System.Collections.ObjectModel;
using System.Windows.Input;

using Boards_WP.Data.Models;

namespace Boards_WP.ViewModels
{
    public class BetsViewModel
    {
        
        public ObservableCollection<Bet> FilteredBets { get; set; } = new();

        private MainViewModel _mainViewModel;
        public MainViewModel MainViewModel => _mainViewModel;

        public double HomeTabOpacity => 1.0;
        public double UserBetsTabOpacity => 0.6;

       
        public ICommand ShowHomeCommand { get; set; }
        public ICommand ShowUserBetsCommand { get; set; }
        public ICommand CreateBetCommand { get; set; }

        public BetsViewModel()
        {
            _mainViewModel = App.GetService<MainViewModel>();

            FilteredBets.Add(new Bet
            {
                Expression = "Mock Bet for UI Testing",
                BetCommunity = new Community { Name = "Test Lab" },
                Type = BetType.Post
            });
        }
    }
}