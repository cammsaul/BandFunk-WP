using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace BandFunk
{
    public class Artist : IComparable<Artist>
    {
        public string Name { get; set; }
        public string EchoNestArtistID { get; set; }
        public string Bio { get; set; }
        public ObservableCollection<Event> Events { get; set; }
        public ObservableCollection<string> RelatedArtistNames { get; set; }
        public Uri TileImageUri { get; set; }
        public Uri MainImageUri { get; set; }
        public ObservableCollection<string> Genres { get; set; }
        public ObservableCollection<NewsItem> NewsItems { get; set; }
        public DateTime LastFetchDate { get; set; }
        public Uri LastFMUri { get; set; }
        public List<Event> NewEvents { get; set; }

        public string UppercaseName
        {
            get
            {
                return Name.ToUpper();
            }
        }

        public string CapitalizedName
        {
            get
            {
                return Utility.TitleCaseString(Name);
            }
        }

        //public BitmapImage MainImage { get { return new BitmapImage(MainImageUri); } }
        public bool IsFetchingEvents { get; set; }
        public bool IsFetchingSimilarArtists { get; set; }

        public bool IsFavorite
        {
            get
            {
                return ArtistStore.Favorites.Contains(this);
            }
        }

        public string LiveTileBackText
        {
            get
            {
                var currentLocation = CurrentLocationManager.CurrentLocation;
                if (currentLocation == null) 
                {
                    return null;
                }
                foreach (var ev in Events)
                {
                    if (ev.Venue.Latitude != null && ev.Venue.Longitude != null)
                    {
                        if (Utility.DistanceBetweenCoordinates(currentLocation.Latitude, currentLocation.Longitude, ev.Venue.Latitude.Value, ev.Venue.Longitude.Value)
                            < CurrentLocationManager.NEARBY_DIST_THRESHOLD_MILES)
                        {
                            return String.Format("{0}\n{1}\n{2}\n{3}", ev.Title, ev.Date.ToShortDateString(), ev.Venue.Title, ev.Venue.City);
                        }
                    }
                }
                return null;
            }
        }

        public int CompareTo(Artist other)
        {
            return Name.CompareTo(other.Name);
        }

        public Artist()
        {
            Events = new ObservableCollection<Event>();
            RelatedArtistNames = new ObservableCollection<string>();
            NewsItems = new ObservableCollection<NewsItem>();
            Genres = new ObservableCollection<string>();
            NewEvents = new List<Event>();
        }

        public override string ToString()
        {
            return String.Format("{0}. Events: {1}, RelatedArtists: {2}, Genres: {3}, NewsItems: {4}", Name, Events.Count, RelatedArtistNames.Count, Genres.Count, RelatedArtistNames.Count);
        }
    }
}
