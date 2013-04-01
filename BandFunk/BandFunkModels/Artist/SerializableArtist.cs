using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace BandFunk
{
    public class SerializableArtist
    {
        public string Name { get; set; }
        public string EchoNestArtistID { get; set; }
        public string Bio { get; set; }
        public ObservableCollection<SerializableEvent> Events { get; set; }
        public ObservableCollection<string> RelatedArtistNames { get; set; }
        public Uri TileImageUri { get; set; }
        public Uri MainImageUri { get; set; }
        public ObservableCollection<string> Genres { get; set; }
        public ObservableCollection<NewsItem> NewsItems { get; set; }
        public DateTime LastFetchDate { get; set; }
        public Uri LastFMUri { get; set; }
        public bool IsFavorite { get; set; }

        public SerializableArtist() { }

        public SerializableArtist(Artist artist)
        {
            Name = artist.Name;
            EchoNestArtistID = artist.EchoNestArtistID;
            Bio = artist.Bio;
            Events = new ObservableCollection<SerializableEvent>();
            foreach (var ev in artist.Events)
            {
                Events.Add(new SerializableEvent(ev));
            }
            RelatedArtistNames = artist.RelatedArtistNames;
            TileImageUri = artist.TileImageUri;
            MainImageUri = artist.MainImageUri;
            Genres = artist.Genres;
            NewsItems = artist.NewsItems;
            LastFetchDate = artist.LastFetchDate;
            LastFMUri = artist.LastFMUri;
            IsFavorite = ArtistStore.Favorites.Contains(artist);
        }

        public Artist ToArtist()
        {
            var artist = new Artist();
            artist.Name = Name;
            artist.EchoNestArtistID = EchoNestArtistID;
            artist.Bio = Bio;
            foreach (var ev in Events)
            {
                // if an event happed before today then don't add it, it has already passed
                if ((ev.Date - DateTime.Today).Days >= 0)
                {
                    artist.Events.Add(ev.ToEvent());
                }
            }
            artist.RelatedArtistNames = RelatedArtistNames;
            artist.TileImageUri = TileImageUri;
            artist.MainImageUri = MainImageUri;
            artist.Genres = Genres;
            artist.NewsItems = NewsItems;
            artist.LastFetchDate = LastFetchDate;
            artist.LastFMUri = LastFMUri;
            return artist;
        }       
    }
}
