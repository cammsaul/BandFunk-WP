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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

namespace BandFunk
{
    public partial class MainPage : PhoneApplicationPage
    {

        public ObservableCollection<ArtistRow> ArtistItems = new ObservableCollection<ArtistRow>();        

        public class ArtistRow
        {
            public string Artist1Name { get; set; }
            public Uri Artist1Image { get; set; }
            public string Artist2Name { get; set; }
            public Uri Artist2Image { get; set; }
            public Visibility Artist2GridVisibility { get; set; }
            public bool Artist1EnableAddTile { get { return ArtistTileUpdater.EnableAddLiveTile(ArtistStore.Get(Artist1Name)); } }
            public bool Artist2EnableAddTile { get { return ArtistTileUpdater.EnableAddLiveTile(ArtistStore.Get(Artist2Name)); } }
        }

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            ArtistListBox.ItemsSource = ArtistItems;
            ShowsListBox.ItemsSource = EventStore.NearbyFavoriteEvents;
            AllShowsListBox.ItemsSource = EventStore.AllNearbyEvents;

            CurrentLocationTextBlock.Text = CurrentLocationManager.CurrentLocationName;
            CurrentLocationManager.CurrentLocationChanged += delegate
            {                
                CurrentLocationTextBlock.Text = CurrentLocationManager.CurrentLocationName;
            };

            ArtistFetcher.ArtistFetched += artist =>
            {
                if (artist == null || artist.IsFavorite)
                {
                    ProgressBar.Visibility = Visibility.Collapsed;                    
                }
                if (artist == null && FetcherLastFM.InvalidArtistName != null)
                {
                    MessageBox.Show(String.Format("Uh oh! We couldn't find any artists named '{0}'.", FetcherLastFM.InvalidArtistName));
                }
            };

            ArtistFetcher.ArtistPartiallyFetched += artist =>
            {
                if (artist != null && artist.IsFavorite && ProgressBar.Visibility == Visibility.Visible)
                {
                    ScrollToArtist(artist);
                }
            };

            ArtistStore.Favorites.CollectionChanged += (sender, e) =>
            {
                ReloadArtistRows();
            };
            
            EventStore.NearbyEventsFetched += delegate
            {
                ProgressBar.Visibility = Visibility.Collapsed;
            };
            EventStore.FetchNearbyEvents();

            if (CurrentLocationManager.EnableCurrentLocation)
            {
                CurrentLocationManager.UpdateCurrentLocation();
            }
            else
            {
                ((from ApplicationBarMenuItem menuItem in ApplicationBar.MenuItems where menuItem.Text == "disable location services" select menuItem)
                    .First()).Text = "enable location services";
                ((from ApplicationBarIconButton button in ApplicationBar.Buttons where button.Text == "update city" select button)
                    .First()).IsEnabled = false;
            } 
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            ReloadArtistRows();

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

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            MainTileUpdater.UpdateMainTile(forceUpdateBackgroundImages:true);
        }

        public void ReloadArtistRows()
        {
            ArtistItems.Clear();
            var artists = ArtistStore.SortedFavorites;
            for (int i = 0; i < artists.Count; i += 2)
            {
                var artist1 = artists[i];
                var artist2 = i + 1 != artists.Count ? artists[i + 1] : null;
                var row = new ArtistRow()
                {
                    Artist1Name = artist1.Name,
                    Artist1Image = artist1.MainImageUri,
                    Artist2Name = artist2 != null ? artist2.Name : null,
                    Artist2Image = artist2 != null ? artist2.MainImageUri : null,
                    Artist2GridVisibility = artist2 != null ? Visibility.Visible : Visibility.Collapsed,
                };                
                ArtistItems.Add(row);
            }            
        }

        private void ArtistGrid_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var grid = sender as Grid;
            TextBlock textblock = null;
            foreach (var child in grid.Children) {
                if (child as TextBlock != null)
                {
                    textblock = child as TextBlock;
                }
            }
            var artistName = textblock.Text;
            NavigationService.Navigate(new Uri("/Views+Pages/ArtistPivotPage.xaml?artistName=" + Uri.EscapeDataString(artistName), UriKind.Relative));
        }

        private void EventGrid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var ev = (sender as Grid).DataContext as Event;
            NavigationService.Navigate(new Uri(String.Format("/Views+Pages/EventPage.xaml?eventID={0}", ev.Id), UriKind.Relative));
        }

        private void AddArtistButton_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
            AddArtist();
        }

        private void AddArtistTextBox_GotFocus_1(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox.Text != null && textBox.Text == "add artist")
            {
                textBox.Text = String.Empty;
            }
            textBox.Background = new SolidColorBrush(Color.FromArgb(255, 240, 84, 36));
            textBox.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)); // black
        }

        private void AddArtistTextBox_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddArtist();
            }
        }

        private void AddArtist()
        {
            if (ArtistStore.Favorites.Count >= 10 && App.IsTrial)
            {
                App.ShowTrialUpsell();
                return;
            }

            if (AddArtistTextBox.Text != "add artist")
            {
                var existing = ArtistStore.Get(AddArtistTextBox.Text);
                if (existing != null && ArtistStore.Favorites.Contains(existing))
                {
                    MessageBox.Show("You've already added " + AddArtistTextBox.Text);
                    AddArtistTextBox.Text = "add artist";
                    return;
                }

                ProgressBar.Visibility = Visibility.Visible;
                if (existing == null)
                {
                    ArtistFetcher.FetchArtist(AddArtistTextBox.Text, true);
                }
                else
                {
                    ArtistStore.Favorites.Add(existing);
                    ArtistFetcher.UpdateArtist(existing);
                    ScrollToArtist(existing);
                }
                AddArtistTextBox.Text = "add artist";
            }
            this.Focus();
        }

        private void ScrollToArtist(Artist artist)
        {
            // find out what row the artist is in
            var row = ArtistStore.SortedFavorites.IndexOf(artist) / 2;
            var listBoxItem = ArtistListBox.Items[row];
            ArtistListBox.ScrollIntoView(listBoxItem);
        }

        private void ContextMenu_Stop_Following_Artist1_Click_1(object sender, RoutedEventArgs e)
        {
            var row = (sender as MenuItem).DataContext as ArtistRow;
            var artistName = row.Artist1Name;
            ArtistStore.Favorites.Remove(ArtistStore.Get(artistName));
        }

        private void ContextMenu_Stop_Following_Artist2_Click_1(object sender, RoutedEventArgs e)
        {
            var row = (sender as MenuItem).DataContext as ArtistRow;
            var artistName = row.Artist2Name;
            ArtistStore.Favorites.Remove(ArtistStore.Get(artistName));
        }

        private void ContextMenu_PinToStart_Artist1_Click_1(object sender, RoutedEventArgs e)
        {
            var row = (sender as MenuItem).DataContext as ArtistRow;
            ArtistTileAdder.PinArtistToStart(ArtistStore.Get(row.Artist1Name));
        }

        private void ContextMenu_PinToStart_Artist2_Click_1(object sender, RoutedEventArgs e)
        {
            var row = (sender as MenuItem).DataContext as ArtistRow;
            ArtistTileAdder.PinArtistToStart(ArtistStore.Get(row.Artist2Name));
        }

        private void ContextMenu_Share_Artist2_Click_1(object sender, RoutedEventArgs e)
        {
            var row = (sender as MenuItem).DataContext as ArtistRow;
            ShareManager.Share(ArtistStore.Get(row.Artist2Name));
        }

        private void ContextMenu_Share_Artist1_Click_1(object sender, RoutedEventArgs e)
        {
            var row = (sender as MenuItem).DataContext as ArtistRow;
            ShareManager.Share(ArtistStore.Get(row.Artist1Name));
        }

        private void ContextMenu_Share_Artist2_Text_Message_Click_1(object sender, RoutedEventArgs e)
        {
            var row = (sender as MenuItem).DataContext as ArtistRow;
            ShareManager.ShareTextMessage(ArtistStore.Get(row.Artist2Name));
        }

        private void ContextMenu_Share_Artist2_Email_Click_1(object sender, RoutedEventArgs e)
        {
            var row = (sender as MenuItem).DataContext as ArtistRow;
            ShareManager.ShareEmail(ArtistStore.Get(row.Artist2Name));
        }

        private void ContextMenu_Share_Artist1_Text_Message_Click_1(object sender, RoutedEventArgs e)
        {
            var row = (sender as MenuItem).DataContext as ArtistRow;
            ShareManager.ShareTextMessage(ArtistStore.Get(row.Artist1Name));
        }

        private void ContextMenu_Share_Artist1_Email_Click_1(object sender, RoutedEventArgs e)
        {
            var row = (sender as MenuItem).DataContext as ArtistRow;
            ShareManager.ShareEmail(ArtistStore.Get(row.Artist1Name));
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

        private void UpdateGPSButton_Click(object sender, EventArgs e)
        {
            CurrentLocationManager.UpdateCurrentLocation();
        }

        private void DisableLocationServicesBarMenuItem_Click_1(object sender, EventArgs e)
        {            
            const string disableStr = "disable location services";
            const string enableStr = "enable location services";

            var barMenuItem = (ApplicationBarMenuItem)sender;
            bool willDisable = barMenuItem.Text == disableStr;
            CurrentLocationManager.EnableCurrentLocation = !willDisable;

            barMenuItem.Text = willDisable ? enableStr : disableStr;
            ((from ApplicationBarIconButton button in ApplicationBar.Buttons where button.Text == "update city" select button)
                .First()).IsEnabled = !willDisable;
            if (willDisable)
            {
                MessageBox.Show("BandFunk uses your current location to determine which shows are near you. This information is never used to identify you in any way. If you disable location services, BandFunk will not be able to tell you which shows are nearby.");
            }
            else
            {
                CurrentLocationManager.UpdateCurrentLocation();
            }
        }

        private void PrivacyPolicyBarMenuItem_Click_1(object sender, EventArgs e)
        {
            MessageBox.Show("BandFunk uses your current location to determine which shows are nearby you. Your location is not used in any other way and is not used to identify you in any way.\n\nPlease email cameron@getluckybird.com with any questions or concerns regarding the privacy policy. ");
        }

        private void VisitLuckyBirdOnlineBarMenuItem_Click_1(object sender, EventArgs e)
        {
            var webBrowserTask = new WebBrowserTask();
            webBrowserTask.Uri = new Uri("http://www.getluckybird.com");
            webBrowserTask.Show();
        }

        private void PurchaseAppButton_Click(object sender, EventArgs e)
        {
            App.ShowOnMarketplace();
        }

        private void AddArtistTextBox_LostFocus_1(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            textBox.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 240, 84, 36)); // black
        }
    }
}