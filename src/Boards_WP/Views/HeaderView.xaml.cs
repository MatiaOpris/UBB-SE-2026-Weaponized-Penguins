using System.ComponentModel;

using Boards_WP.ViewModels;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Boards_WP.Views
{
    public sealed partial class HeaderView : UserControl
    {
        public HeaderViewModel ViewModel { get; private set; }
        private readonly IBetsService betsService;
        private readonly UserSession userSession;
        private Frame contentFrame;

        public HeaderView()
        {
            this.InitializeComponent();
            betsService = App.GetService<IBetsService>();
            userSession = App.GetService<UserSession>();

            this.Loaded += HeaderView_Loaded;
            this.Unloaded += HeaderView_Unloaded;
        }

        private void HeaderView_Loaded(object sender, RoutedEventArgs e)
        {
            if (Application.Current is App myApp)
            {
                this.ViewModel = App.GetService<HeaderViewModel>();
                this.DataContext = this.ViewModel;

                this.ViewModel.PropertyChanged += ViewModel_PropertyChanged;

                TokenEvents.TokensUpdated += OnTokensUpdated;

                contentFrame = myApp.M_window?.Content is FrameworkElement fe
                    ? fe.FindName("ContentFrame") as Frame
                    : null;

                if (contentFrame != null)
                {
                    contentFrame.Navigated -= ContentFrame_Navigated;
                    contentFrame.Navigated += ContentFrame_Navigated;
                    UpdateTokenVisibility(contentFrame.Content?.GetType());
                }

                this.Bindings.Update();
            }
        }

        private void HeaderView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel != null)
            {
                this.ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }

            TokenEvents.TokensUpdated -= OnTokensUpdated;
        }

        private void OnTokensUpdated(int newAmount)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                TokenCountText.Text = newAmount.ToString();
            });
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(HeaderViewModel.UserTokens))
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    TokenCountText.Text = ViewModel.UserTokens.ToString();
                });
            }
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            UpdateTokenVisibility(e.SourcePageType);
        }

        private void UpdateTokenVisibility(Type currentPageType)
        {
            var inBetsArea = currentPageType == typeof(Pages.BetsView)
                || currentPageType == typeof(Pages.CreateBetView)
                || currentPageType == typeof(Pages.PlaceBetView);

            TokenDisplay.Visibility = inBetsArea ? Visibility.Visible : Visibility.Collapsed;

            if (inBetsArea && ViewModel != null)
            {
                var userId = userSession?.CurrentUser?.UserID ?? 0;
                if (userId != 0)
                {
                    try
                    {
                        ViewModel.UserTokens = betsService.GetUserTokenCount(userId);
                    }
                    catch
                    {
                    }
                }
            }
        }

        private void CommunitySearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && ViewModel != null)
            {
                ViewModel.SearchText = sender.Text;
            }
        }

        private void CommunitySearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem is Community selectedCommunity && ViewModel != null)
            {
                if (selectedCommunity.CommunityID == -1)
                {
                    return;
                }

                ViewModel.SelectCommunityCommand.Execute(selectedCommunity);
                sender.Text = string.Empty;
            }
        }

        private void CommunitySearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (ViewModel == null)
            {
                return;
            }

            string query = sender.Text.ToLower().Trim();

            if (betsService.IsSecretKey(query))
            {
                var userId = userSession?.CurrentUser?.UserID ?? 0;
                if (userId != 0)
                {
                    var tokens = betsService.RegisterSecretAreaVisitAndGetTokens(userId);
                    ViewModel.UserTokens = tokens;
                }

                TokenDisplay.Visibility = Visibility.Visible;
                var keywords = betsService.ExtractBetKeywords(query);

                NavigateToPage(typeof(Pages.BetsView), keywords);

                sender.Text = string.Empty;
                ViewModel.SearchText = string.Empty;
                return;
            }
            if (args.ChosenSuggestion is Community selected)
            {
                ViewModel.SelectCommunityCommand.Execute(selected);
                sender.Text = string.Empty;
            }
        }

        private void NavigateToPage(Type pageType, object parameter)
        {
            var rootFrame = (App.Current as App)?.M_window?.Content as Frame;
            if (rootFrame == null && (App.Current as App)?.M_window?.Content is FrameworkElement fe)
            {
                rootFrame = fe.FindName("ContentFrame") as Frame;
            }
            rootFrame?.Navigate(pageType, parameter);
        }

        private void ResultsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Community selectedCommunity && ViewModel != null)
            {
                ViewModel.SelectCommunityCommand.Execute(selectedCommunity);
                // ResultsPopup.IsOpen = false;
            }
        }

        public Visibility GetVisibility(bool showText)
        {
            return showText ? Visibility.Visible : Visibility.Collapsed;
        }

        public static Visibility GetVisibilityFromId(int id)
        {
            return id == -1 ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}