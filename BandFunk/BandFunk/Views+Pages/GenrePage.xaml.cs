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
using Microsoft.Phone.Shell;

namespace BandFunk
{
    public partial class GenrePage : PhoneApplicationPage
    {
        private string GenreName { get; set; }
        private Genre _Genre;
        private Genre Genre
        {
            get
            {
                return _Genre;
            }
            set
            {
                _Genre = value;
                DataContext = Genre;
                ReloadArtistRows();
            }
        }
        public ObservableCollection<ArtistRow> ArtistItems = new ObservableCollection<ArtistRow>();

        public class ArtistRow
        {
            public string Artist1Name { get; set; }
            public ImageSource Artist1Image { get; set; }
            public string Artist2Name { get; set; }
            public ImageSource Artist2Image { get; set; }
            public Visibility Artist2GridVisibility { get; set; }
        }

        public GenrePage()
        {
            InitializeComponent();

            ArtistListBox.ItemsSource = ArtistItems;

            GenreStore.GenrePartiallyFetched += genre =>
                {
                    if (genre != null && genre.Name.Equals(GenreName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Genre = genre;
                    }
                };

            GenreStore.GenreFetched += genre =>
                {
                    if (genre == null)
                    {
                        ProgressBar.Visibility = Visibility.Collapsed;
                        return;
                    }

                    if (genre.Name.Equals(GenreName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        ReloadArtistRows();
                        ProgressBar.Visibility = Visibility.Collapsed;

                        foreach (ApplicationBarIconButton appBarButton in ApplicationBar.Buttons)
                        {
                            if (appBarButton.Text == "pin to start")
                            {
                                appBarButton.IsEnabled = GenreTileManager.EnableAddLiveTile(genre);
                            }
                            else
                            {
                                appBarButton.IsEnabled = true;
                            }
                        }
                    }
                };
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            GenreName = NavigationContext.QueryString["genre"].ToLower();
            Genre = GenreStore.Get(GenreName);
            ProgressBar.Visibility = Genre == null ? Visibility.Visible : Visibility.Collapsed;
            if (Genre == null)
            {
                GenreStore.FetchGenre(GenreName);
            }
            else
            {
                foreach (ApplicationBarIconButton appBarButton in ApplicationBar.Buttons)
                {
                    if (appBarButton.Text == "pin to start")
                    {
                        appBarButton.IsEnabled = GenreTileManager.EnableAddLiveTile(Genre);
                    }
                }
            }
            
        }

        public void ReloadArtistRows()
        {
            ArtistItems.Clear();
            if (Genre != null)
            {
                var artists = Genre.TopArtists;
                for (int i = 0; i < artists.Count; i += 2)
                {
                    var artist1 = artists[i];
                    var artist2 = i + 1 != artists.Count ? artists[i + 1] : null;
                    var row = new ArtistRow()
                    {
                        Artist1Name = artist1.Name,
                        Artist1Image = artist1.Image,
                        Artist2Name = artist2 != null ? artist2.Name : null,
                        Artist2Image = artist2 != null ? artist2.Image : null,
                        Artist2GridVisibility = artist2 != null ? Visibility.Visible : Visibility.Collapsed,
                    };
                    ArtistItems.Add(row);
                }
            }
        }

        private void ArtistGrid_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var grid = sender as Grid;
            TextBlock textblock = null;
            foreach (var child in grid.Children)
            {
                if (child as TextBlock != null)
                {
                    textblock = child as TextBlock;
                }
            }
            var artistName = textblock.Text;
            NavigationService.Navigate(new Uri("/Views+Pages/ArtistPivotPage.xaml?artistName=" + Uri.EscapeDataString(artistName), UriKind.Relative));
        }

        private void GenreTextBlock_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var genreName = (sender as TextBlock).Text;
            NavigationService.Navigate(new Uri(String.Format("/Views+Pages/GenrePage.xaml?genre={0}", Uri.EscapeDataString(genreName.ToLower())), UriKind.Relative));
        }

        private void PinToStartButton_Click(object sender, EventArgs e)
        {
            GenreTileManager.PinToStart(Genre);
        }

        private void ShareButton_Click(object sender, EventArgs e)
        {
            ShareManager.Share(Genre);
        }

        private void ShareTextMessageBarMenuItem_Click_1(object sender, EventArgs e)
        {
            ShareManager.ShareTextMessage(Genre);
        }

        private void ShareEmailBarMenuItem_Click_1(object sender, EventArgs e)
        {
            ShareManager.ShareEmail(Genre);
        }
    }
}