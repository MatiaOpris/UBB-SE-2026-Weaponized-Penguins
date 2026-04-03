using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;

using Boards_WP.ViewModels;
using Boards_WP.Data.Models;

namespace Boards_WP.Views
{
    public sealed partial class HeaderView : UserControl
    {
        public HeaderViewModel ViewModel { get; private set; }

        public HeaderView()
        {
            this.InitializeComponent();

            this.Loaded += HeaderView_Loaded;
        }

        private void HeaderView_Loaded(object sender, RoutedEventArgs e)
        {
            if (Application.Current is App myApp)
            {
                this.ViewModel = App.GetService<HeaderViewModel>();
                this.DataContext = this.ViewModel;

                this.Bindings.Update();
            }
        }

        private void CommunitySearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && ViewModel != null)
            {
                string query = sender.Text.ToLower().Trim();

                if (query == "/weaponizedpenguins")
                {
                    TokenDisplay.Visibility = Visibility.Visible;
                    sender.Text = string.Empty;
                    ViewModel.UserTokens += 5;
                    NavigateToPage(typeof(Pages.BetsView), null);

                    return;
                }

                ViewModel.SearchText = sender.Text;
            }
        }

        private void CommunitySearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem is Community selectedCommunity && ViewModel != null)
            {
                if (selectedCommunity.CommunityID == -1) return;

                ViewModel.SelectCommunityCommand.Execute(selectedCommunity);
                sender.Text = string.Empty; 
            }
        }

        private void NavigateToPage(Type pageType, object parameter)
        {
            var rootFrame = (App.Current as App)?.m_window?.Content as Frame;
            if (rootFrame == null && (App.Current as App)?.m_window?.Content is FrameworkElement fe)
            {
                rootFrame = fe.FindName("ContentFrame") as Frame;
            }
            rootFrame?.Navigate(pageType, parameter);
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