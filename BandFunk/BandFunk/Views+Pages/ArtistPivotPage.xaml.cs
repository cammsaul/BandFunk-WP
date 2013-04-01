using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Shell;
using System.Diagnostics;

namespace BandFunk
{
    public partial class ArtistPivotPage : PhoneApplicationPage
    {
        private string ArtistName;
        private Artist Artist;

        public ArtistPivotPage()
        {
            InitializeComponent();

            ArtistFetcher.ArtistFetched += artist =>
                {
                    if (artist == null)
                    {
                        Artist = null;
                        ProgressBar.Visibility = Visibility.Collapsed;
                        return;
                    }
                    if (artist.Name.Equals(ArtistName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Artist = artist;
                        UpdateAppBarButtons();
                        ProgressBar.Visibility = Visibility.Collapsed;
                    }
                };
            ArtistFetcher.ArtistPartiallyFetched += artist =>
                {
                    if (artist.Name.Equals(ArtistName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Artist = artist;
                        LayoutRoot.DataContext = Artist;                        
                    }
                };
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            ArtistName = NavigationContext.QueryString["artistName"].ToLower();
            Artist = ArtistStore.Get(ArtistName);
            if (Artist != null)
            {
                ProgressBar.Visibility = Visibility.Collapsed;
                UpdateAppBarButtons();

                var lastFetch = (DateTime.Now - Artist.LastFetchDate).Days;
                if (lastFetch > 1)
                {
                    ProgressBar.Visibility = Visibility.Visible;
                    Debug.WriteLine("Artist {0} out of date ({1} days ago), updating...", Artist.Name, lastFetch);
                    ArtistFetcher.UpdateArtist(Artist);
                }
            }
            else
            {
                foreach (ApplicationBarIconButton button in ApplicationBar.Buttons)
                {
                    if (button.Text != "buy full app")
                    {
                        button.IsEnabled = false;
                    }
                }
                // fetch the artist!
                Pivot.Title = ArtistName.ToUpper();
                ArtistFetcher.FetchArtist(ArtistName);
            }

            LayoutRoot.DataContext = Artist;

            foreach (ApplicationBarIconButton button in ApplicationBar.Buttons)
            {
                if (button.Text == "buy full app")
                {
                    if (!App.IsTrial)
                    {
                        ApplicationBar.Buttons.Remove(button);
                        break;
                    }
                }
            }
        }

        private void UpdateAppBarButtons()
        {
            foreach (ApplicationBarIconButton button in ApplicationBar.Buttons)
            {
                if (button.Text == "follow")
                {
                    button.IsEnabled = !Artist.IsFavorite;
                }
                else if (button.Text == "pin to start")
                {
                    button.IsEnabled = ArtistTileUpdater.EnableAddLiveTile(Artist);
                }
                else
                {
                    button.IsEnabled = true;
                }
            }
        }

        private void EventGrid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var ev = (sender as Grid).DataContext as Event;
            NavigationService.Navigate(new Uri(String.Format("/Views+Pages/EventPage.xaml?eventID={0}", ev.Id), UriKind.Relative));
        }

        private void NewsItemStackPanel_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var newsItem = (sender as StackPanel).DataContext as NewsItem;
            var webBrowserTask = new WebBrowserTask();
            webBrowserTask.Uri = newsItem.Uri;
            webBrowserTask.Show();
        }

        private void RelatedArtistTextBlock_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var artistName = (sender as TextBlock).Text;
            NavigationService.Navigate(new Uri(String.Format("/Views+Pages/ArtistPivotPage.xaml?artistName={0}", Uri.EscapeDataString(artistName)), UriKind.Relative));
        }

        private void GenreTextBlock_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var genreName = (sender as TextBlock).Text;
            NavigationService.Navigate(new Uri(String.Format("/Views+Pages/GenrePage.xaml?genre={0}", Uri.EscapeDataString(genreName.ToLower())), UriKind.Relative));
        }

        private void FollowButton_Click(object sender, EventArgs e)
        {
            if (ArtistStore.Favorites.Count >= 10 && App.IsTrial)
            {
                App.ShowTrialUpsell();
                return;
            }

            ArtistStore.Favorites.Add(Artist);
            UpdateAppBarButtons();
        }

        private void ContextMenu_Share_Event_Click_1(object sender, RoutedEventArgs e)
        {
            ShareManager.Share((sender as MenuItem).DataContext as Event);
        }

        private void ContextMenu_PinToStart_Event_Click_1(object sender, RoutedEventArgs e)
        {
            var anEvent = (sender as MenuItem).DataContext as Event;
            EventTileManager.PinToStart(anEvent);
        }

        private void ContextMenu_Share_Event_Text_Message_Click_1(object sender, RoutedEventArgs e)
        {
            var anEvent = (sender as MenuItem).DataContext as Event;
            ShareManager.ShareTextMessage(anEvent);
        }

        private void ContextMenu_Share_Event_Email_Click_1(object sender, RoutedEventArgs e)
        {
            var anEvent = (sender as MenuItem).DataContext as Event;
            ShareManager.ShareEmail(anEvent);
        }

        private void PinToStartButton_Click(object sender, EventArgs e)
        {
            ArtistTileAdder.PinArtistToStart(Artist);
            UpdateAppBarButtons();
        }

        private void ShareArtistButton_Click_1(object sender, EventArgs e)
        {
            ShareManager.Share(Artist);
        }

        private void ShareArtistViaTextMessageBarMenuItem_Click_1(object sender, EventArgs e)
        {
            ShareManager.ShareTextMessage(Artist);
        }

        private void ShareArtistViaEmailBarMenuItem_Click_1(object sender, EventArgs e)
        {
            ShareManager.ShareEmail(Artist);
        }

        private void ContextMenu_Share_NewsItem_Click_1(object sender, RoutedEventArgs e)
        {
            var newsItem = (sender as MenuItem).DataContext as NewsItem;
            var shareLinkTask = new ShareLinkTask();
            shareLinkTask.Title = newsItem.Name;
            shareLinkTask.LinkUri = newsItem.Uri;
            shareLinkTask.Message = newsItem.Summary;
            shareLinkTask.Show();
        }

        private void ContextMenu_Share_NewsItem_TextMessage_Click_1(object sender, RoutedEventArgs e)
        {
            var newsItem = (sender as MenuItem).DataContext as NewsItem;
            var smsComposeTask = new SmsComposeTask();
            smsComposeTask.Body = String.Format("{0}\n\n{1}", newsItem.Name, newsItem.Uri);
            smsComposeTask.Show();
        }

        private void ContextMenu_Share_NewsItem_Email_Click_1(object sender, RoutedEventArgs e)
        {
            var newsItem = (sender as MenuItem).DataContext as NewsItem;
            var emailComposeTask = new EmailComposeTask();
            emailComposeTask.Subject = newsItem.Name;
            emailComposeTask.Body = String.Format("{0}\n\n{1}", newsItem.Summary, newsItem.Uri);
            emailComposeTask.Show();
        }

        private void PurchaseAppButton_Click(object sender, EventArgs e)
        {
            App.ShowOnMarketplace();
        }
    }
}