using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace BandFunk
{
    public class SerializableEvent
    {
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public Uri ImageUri { get; set; }
        public Uri LastFMUri { get; set; }
        public SerializableVenue Venue { get; set; }
        public ObservableCollection<string> OtherArtists { get; set; }
        public string Id { get; set; }
        public string Headliner { get; set; }

        public SerializableEvent() { }

        public SerializableEvent(Event ev)
        {
            Title = ev.Title;
            Date = ev.Date;
            ImageUri = ev.ImageUri;
            LastFMUri = ev.LastFMUri;
            Venue = new SerializableVenue(ev.Venue);
            OtherArtists = ev.OtherArtists;
            Id = ev.Id;
            Headliner = ev.Headliner;
        }

        public Event ToEvent()
        {
            var ev = new Event();
            ev.Title = Title;
            ev.Date = Date;
            ev.ImageUri = ImageUri;
            ev.LastFMUri = LastFMUri;
            ev.OtherArtists = OtherArtists;
            ev.Id = Id;
            ev.Headliner = Headliner;
            ev.Venue = Venue.ToVenue();
            return ev;
        }
    }
}
