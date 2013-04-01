using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Media.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using System.Device.Location;
using System.Collections.ObjectModel;
using System.Windows;

namespace BandFunk
{
    public static class FetcherLastFM
    {
        public delegate void ArtistFetchedDelegate(Artist artist);
        public static ArtistFetchedDelegate ArtistFetched;

        public delegate void EventsForArtistFetchedDelegate(Artist artist);
        public static EventsForArtistFetchedDelegate EventsForArtistFetched;

        /// <summary>
        /// As of right now this is only called as a result of the FetchEvent() method -- it is not called for FetchEventsForArtist or FetchNearbyEvents
        /// </summary>
        public delegate void EventFetchedDelegate(Event anEvent);
        public static EventFetchedDelegate EventFetched;

        public static string InvalidArtistName { get; set; }

        private const string API_KEY = "7e608454ab11cae9d75726ea6258ca40";

        public static void FetchEvent(string eventId)
        {
            var request = String.Format("http://ws.audioscrobbler.com/2.0/?method=event.getInfo&event={0}&api_key={1}&format=json", eventId, API_KEY);
            Debug.WriteLine("LastFM: Fetching Event {0}: {1}", eventId, request);
            var webClient = new WebClient();
            var userToken = new Utility.WebClientToken();
            //userToken.MakeFavorite = makeFavorite;
            webClient.DownloadStringCompleted += webClient_FetchEvent_DownloadStringCompleted;
            webClient.DownloadStringAsync(new Uri(request, UriKind.Absolute), userToken);
        }

        private static void webClient_FetchEvent_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            JObject json = JObject.Parse(e.Result);

            var anEvent = ParseEvent(json["event"]);
            if (EventFetched != null)
            {
                if (EventStore.Events.ContainsKey(anEvent.Id))
                {
                    EventStore.Events[anEvent.Id] = anEvent;
                }
                else
                {
                    EventStore.Events.Add(anEvent.Id, anEvent);
                }
                EventFetched(anEvent);
            }
        }

        public static void FetchArtist(string artistName, bool makeFavorite)
        {
            var request = String.Format("http://ws.audioscrobbler.com/2.0/?method=artist.getInfo&artist={0}&api_key={1}&format=json&autocorrect=1", Uri.EscapeDataString(artistName), API_KEY);
            Debug.WriteLine("LastFM: Fetching Artist {0}: {1}", artistName, request);
            var webClient = new WebClient();
            var userToken = new Utility.WebClientToken();
            userToken.Name = artistName;
            userToken.MakeFavorite = makeFavorite;
            webClient.DownloadStringCompleted += webClient_FetchArtist_DownloadStringCompleted;
            webClient.DownloadStringAsync(new Uri(request, UriKind.Absolute), userToken: userToken);
        }

        private static void webClient_FetchArtist_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                var result = e.Result;
            }
            catch
            {
                Debug.WriteLine("Unable to fetch artist (no internet connection?)");
                if (ArtistFetched != null)
                {
                    ArtistFetched(null);
                }
                return;
            }

            JObject json = JObject.Parse(e.Result);

            if (json["error"] != null)
            {
                var artistName = (e.UserState as Utility.WebClientToken).Name;
                Debug.WriteLine("LastFM: No artist named {0}.", artistName);
                InvalidArtistName = artistName;
                ArtistFetched(null);
                return;
            }

            var artistJson = json["artist"];
            
            string megaArtistImage = null;
            string extraLargeArtistImage = null;
            foreach (var imageJson in artistJson["image"].Children())
            {
                if (imageJson["size"].Value<string>() == "mega")
                {
                    megaArtistImage = imageJson["#text"].Value<string>();
                }
                else if (imageJson["size"].Value<string>() == "extralarge")
                {
                    extraLargeArtistImage = imageJson["#text"].Value<string>();
                }
            }

            var genres = new ObservableCollection<string>();
            try
            {
                foreach (var tagJson in artistJson["tags"]["tag"].Children())
                {
                    genres.Add(tagJson["name"].Value<string>());
                }
            }
            catch { }

            Artist artist = new Artist()
            {
                Name = artistJson["name"].Value<string>().ToLower(),
                LastFMUri = new Uri(artistJson["url"].Value<string>()),
                TileImageUri = extraLargeArtistImage != null && extraLargeArtistImage.Length > 0 ? new Uri(extraLargeArtistImage) : null,
                MainImageUri = megaArtistImage != null && megaArtistImage.Length > 0 ? new Uri(megaArtistImage) : null,
                Genres = genres,
                Bio = Utility.ClearHTMLTagsFromString(HttpUtility.HtmlDecode(artistJson["bio"]["content"].Value<string>()))                ,
            };
            if ((e.UserState as Utility.WebClientToken).MakeFavorite)
            {
                ArtistStore.Favorites.Add(artist);
            }

            Debug.WriteLine("LastFM: Fetched Artist {0}", artist.Name);
            ArtistFetched(artist);
        }

        public static void FetchEventsForArtist(Artist artist)
        {
            var request = String.Format("http://ws.audioscrobbler.com/2.0/?method=artist.getEvents&artist={0}&api_key={1}&format=json&autocorrect=1", Uri.EscapeDataString(artist.Name), API_KEY);
            Debug.WriteLine("LastFM: Fetching Events For Artist {0}: {1}", artist.Name, request);
            var webClient = new WebClient();
            var userToken = new Utility.WebClientToken();
            userToken.Artist = artist;
            webClient.DownloadStringCompleted += webClient_FetchEvents_DownloadStringCompleted;
            webClient.DownloadStringAsync(new Uri(request), userToken);
        }

        static void webClient_FetchEvents_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            var artist = (e.UserState as Utility.WebClientToken).Artist;
            var oldEvents = new List<Event>(artist.Events);
            artist.NewEvents.Clear();
            artist.Events.Clear();

            JObject json = JObject.Parse(e.Result);
            if (json["events"]["event"] != null)
            {
                foreach (var eventJson in json["events"]["event"].Children())
                {
                    try
                    {
                        var ev = ParseEvent(eventJson);
                        artist.Events.Add(ev);
                    }
                    catch { }
                }
            }

            // determine which events are new events
            foreach (var ev in artist.Events)
            {
                if (!oldEvents.Contains(ev))
                {
                    artist.NewEvents.Add(ev);
                }
            }

            Debug.WriteLine("LastFM: Fetched Events For Artist {0}", artist.Name);
            EventsForArtistFetched(artist);
        }

        public static void FetchAllNearbyEvents()
        {
            if (CurrentLocationManager.CurrentLocation == null)
            {
                EventStore.AllNearbyEvents.Clear();
                Debug.WriteLine("LastFM: Current Location Unavailable");
                if (EventStore.NearbyEventsFetched != null)
                {
                    EventStore.NearbyEventsFetched();
                }
                return;
            }
            
            var request = String.Format("http://ws.audioscrobbler.com/2.0/?method=geo.getEvents&api_key={0}&format=json&autocorrect=1&lat={1}&long={2}", API_KEY, 
                CurrentLocationManager.CurrentLocation.Latitude, CurrentLocationManager.CurrentLocation.Longitude);
            Debug.WriteLine("LastFM: Fetching All Nearby Events: {0}", request);
            var webClient = new WebClient();            
            webClient.DownloadStringCompleted += webClient_FetchAllNearbyEvents_DownloadStringCompleted;
            webClient.DownloadStringAsync(new Uri(request));
        }

        static void webClient_FetchAllNearbyEvents_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                var result = e.Result;
            }
            catch
            {
                Debug.WriteLine("Unable to fetch nearby events (no internet connection?)");
                if (EventStore.NearbyEventsFetched != null)
                {
                    EventStore.NearbyEventsFetched();
                }
                return;
            }

            EventStore.AllNearbyEvents.Clear();            

            JObject json = JObject.Parse(e.Result);
            if (json["events"]["event"] != null)
            {
                foreach (var eventJson in json["events"]["event"].Children())
                {
                    try
                    {
                        var ev = ParseEvent(eventJson);
                        EventStore.AllNearbyEvents.Add(ev);
                    }
                    catch { }
                }
            }

            Debug.WriteLine("LastFM: Fetched All Nearby Events");
            if (EventStore.NearbyEventsFetched != null)
            {
                EventStore.NearbyEventsFetched();
            }
        }

        private static Event ParseEvent(JToken eventJson)
        {
            var eventId = eventJson["id"].Value<string>();
            if (EventStore.Events.ContainsKey(eventId))
            {
                return EventStore.Events[eventId];
            }

            var ev = new Event();
            EventStore.Events.Add(eventId, ev);
            ev.Id = eventId;
            ev.Title = eventJson["title"].Value<string>();
            ev.LastFMUri = new Uri(eventJson["url"].Value<string>());
            ev.Headliner = eventJson["artists"]["headliner"].Value<string>();
            DateTime outDate;
            DateTime.TryParse(eventJson["startDate"].Value<string>(), out outDate);
            ev.Date = outDate;
            foreach (var imageJson in eventJson["image"].Children())
            {
                if (imageJson["size"].Value<string>() == "extralarge")
                {
                    var imageStr = imageJson["#text"].Value<string>();
                    if (imageStr != null && imageStr.Length > 0)
                    {
                        ev.ImageUri = new Uri(imageStr);
                    }
                }
            }

            foreach (var artistJson in eventJson["artists"]["artist"].Children())
            {
                var name = artistJson.Value<string>();
                ev.OtherArtists.Add(name);
            }
            if (ev.OtherArtists.Count == 0)
            {
                ev.OtherArtists.Add(ev.Headliner);
            }

            var venue = new Venue();
            ev.Venue = venue;

            var venueJson = eventJson["venue"];
            venue.Title = venueJson["name"].Value<string>();
            //venue.Uri = new Uri(venueJson["url"].Value<string>());
            venue.PhoneNumber = venueJson["phonenumber"].Value<string>();

            var locationJson = venueJson["location"];
            venue.StreetAddress = locationJson["street"].Value<string>();
            venue.City = locationJson["city"].Value<string>();
            venue.Country = locationJson["country"].Value<string>();
            try
            {
                venue.Latitude = locationJson["geo:point"]["geo:lat"].Value<double>();
                venue.Longitude = locationJson["geo:point"]["geo:long"].Value<double>();
            }
            catch { }

            foreach (var imageJson in venueJson["image"].Children())
            {
                if (imageJson["size"].Value<string>() == "mega")
                {
                    var imageStr = imageJson["#text"].Value<string>();
                    if (imageStr != null && imageStr.Length > 0)
                    {
                        venue.ImageUri = new Uri(imageStr);
                        venue.Image = new BitmapImage(venue.ImageUri);
                    }
                }
            }

            return ev;
        }
    }
}
