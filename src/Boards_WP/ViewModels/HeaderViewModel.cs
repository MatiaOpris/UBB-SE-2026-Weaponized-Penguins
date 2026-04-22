using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Boards_WP.ViewModels
{
    public partial class HeaderViewModel : ObservableObject
    {
        private readonly ICommunitiesService communitiesService;
        private readonly INavigationService navigationService;
        private readonly UserSession userSession;
        private readonly MainViewModel mainViewModel;

        public MainViewModel MainViewModel => mainViewModel;

        [ObservableProperty]
        private int userTokens;

        [ObservableProperty]
        private string searchText = string.Empty;

        [ObservableProperty]
        private ObservableCollection<Community> searchResults = new ();

        [ObservableProperty]
        private bool noResultsToggle;

        public HeaderViewModel(ICommunitiesService communitiesService, INavigationService navigationService, UserSession userSession, MainViewModel mainViewModel)
        {
            this.communitiesService = communitiesService;
            this.navigationService = navigationService;
            this.userSession = userSession;
            this.mainViewModel = mainViewModel;

            UserTokens = 1250;
        }

        partial void OnSearchTextChanged(string searchedValue)
        {
            if (string.IsNullOrWhiteSpace(searchedValue))
            {
                SearchResults.Clear();
                NoResultsToggle = false;
                return;
            }

            var matches = communitiesService.searchCommunities(searchedValue);

            SearchResults.Clear();
            foreach (var community in matches)
            {
                SearchResults.Add(community);
            }

            if (SearchResults.Count == 0)
            {
                SearchResults.Add(new Community { CommunityID = -1, Name = "no results are found" });
            }

            NoResultsToggle = (matches.Count == 0);
        }

        [RelayCommand]
        public void SelectCommunity(Community selected)
        {
            if (selected == null)
            {
                return;
            }

            SearchText = string.Empty;
            SearchResults.Clear();

            navigationService.NavigateTo(typeof(Views.Pages.CommunityView), selected);
        }
    }
}