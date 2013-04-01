using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace BandFunk
{
    public class Event : IComparable<Event>
    {
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string FormattedDateString { get { return Date.ToLongDateString(); } }
        public BitmapImage Image { get { return new BitmapImage(ImageUri); } }
        public Uri ImageUri { get; set; }
        public Uri LastFMUri { get; set; }
        public Venue Venue { get; set; }
        public ObservableCollection<string> OtherArtists { get; set; }
        public string Id { get; set; }
        public string Headliner { get; set; }

        public string FavoritesString
        {
            get
            {
                var favorites = new List<Artist>();
                foreach (var artist in ArtistStore.Favorites)
                {
                    if (artist.Events.Contains(this))
                    {
                        favorites.Add(artist);
                    }
                }
                if (favorites.Count == 0)
                {
                    return this.Headliner;
                }
                if (favorites.Count == 1)
                {
                    return favorites[0].UppercaseName;
                }
                else 
                {
                    return String.Format("{0} and {1} other favorites", favorites[0].Name, favorites.Count - 1).ToUpper();
                }
            }
        }

        public Event()
        {
            OtherArtists = new ObservableCollection<string>();
        }

        public override string ToString()
        {
            return Id + ", " + Title;
        }

        public int CompareTo(Event other)
        {
            return Date.CompareTo(other.Date);
        }
    }
}
