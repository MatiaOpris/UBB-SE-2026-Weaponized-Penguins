using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Boards_WP.ViewModels
{
    public partial class CreateBetViewModel : ObservableObject
    {
        private readonly IBetsService betsService;
        private readonly ICommunitiesService communitiesService;
        private readonly UserSession userSession;

        public CreateBetViewModel(IBetsService betsService)
        {
            this.betsService = betsService;
            communitiesService = App.GetService<ICommunitiesService>();
            userSession = App.GetService<UserSession>();

            Communities = new ObservableCollection<Community>();

            LoadCommunities();
        }

        [ObservableProperty]
        private string expression;

        [ObservableProperty]
        private bool isPost = true;

        [ObservableProperty]
        private DateTimeOffset endDate = DateTimeOffset.Now;

        [ObservableProperty]
        private TimeSpan endTime = DateTime.Now.TimeOfDay;

        public ObservableCollection<Community> Communities { get; }

        [ObservableProperty]
        private Community selectedCommunity;

        [ObservableProperty]
        private string errorMessage;

        private void LoadCommunities()
        {
            try
            {
                var communities = communitiesService.GetCommunitiesUserIsPartOf(userSession.CurrentUser.UserID);

                Communities.Clear();

                foreach (var community in communities)
                {
                    Communities.Add(community);
                }

                SelectedCommunity = Communities.FirstOrDefault();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        [RelayCommand]
        private void CreateBet()
        {
            try
            {
                int currentUserId = userSession.CurrentUser.UserID;

                int communityId = SelectedCommunity.CommunityID;

                Bet newBet = new Bet
                {
                    Expression = Expression,
                    StartingTime = DateTime.Now,
                    EndingTime = EndDate.DateTime.Date + EndTime,
                    Type = IsPost ? BetType.Post : BetType.Comment,
                    YesAmount = 0,
                    NoAmount = 0,
                    BetCommunity = SelectedCommunity
                };

                betsService.ValidateCreateBet(currentUserId, newBet);
                betsService.CreateBet(newBet, currentUserId);

                ErrorMessage = null;

                BetCreated?.Invoke();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        public event Action BetCreated;

        [RelayCommand]
        private void Cancel()
        {
            BetCancelled?.Invoke();
        }

        public event Action BetCancelled;
    }
}