using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Device.Location;
using System.Linq;
using System.Text;

namespace BandFunk
{
    public static class EventStore
    {
        /// <summary>
        /// This is keyed by the event "ID" from Last.FM
        /// </summary>
        public static Dictionary<string, Event> Events = new Dictionary<string, Event>();

        public static ObservableCollection<Event> NearbyFavoriteEvents { get; set; }
        public static ObservableCollection<Event> AllNearbyEvents { get; set; }

        public delegate void NearbyEventsFetchedDelegate();
        public static NearbyEventsFetchedDelegate NearbyEventsFetched;

        private static GeoCoordinate LastCurrentLocation = null;

        static EventStore()
        {
            NearbyFavoriteEvents = new ObservableCollection<Event>();
            AllNearbyEvents = new ObservableCollection<Event>();            

            ArtistStore.Favorites.CollectionChanged += (sender, e) =>
                {
                    UpdateNearbyFavorites();
                };
            FetcherLastFM.EventsForArtistFetched += artist =>
                {
                    UpdateNearbyFavorites();
                };
            CurrentLocationManager.CurrentLocationChanged += delegate
                {
                    if (LastCurrentLocation != CurrentLocationManager.CurrentLocation)
                    {
                        LastCurrentLocation = CurrentLocationManager.CurrentLocation;
                        FetchNearbyEvents();
                        UpdateNearbyFavorites();
                    }
                };
        }

        private static void UpdateNearbyFavorites()
        {
            NearbyFavoriteEvents.Clear();
            var eventsList = new List<Event>();

            var currentLocation = CurrentLocationManager.CurrentLocation;
            if (currentLocation == null)
            {
                return;
            }

            foreach (var ev in Events.Values)
            {
                var venue = ev.Venue;
                if (venue.Latitude != null && venue.Longitude != null)
                {
                    if (Utility.DistanceBetweenCoordinates(currentLocation.Latitude, currentLocation.Longitude,
                        venue.Latitude.Value, venue.Longitude.Value) < CurrentLocationManager.NEARBY_DIST_THRESHOLD_MILES)
                    {
                        bool isFavorite = false;
                        foreach (var artist in ArtistStore.Favorites)
                        {
                            foreach (var faveEvent in artist.Events)
                            {
                                if (faveEvent == ev)
                                {
                                    eventsList.Add(ev);
                                    isFavorite = true;
                                    break;
                                }                                
                            }
                            if (isFavorite)
                            {
                                break;
                            }
                        }
                    }
                }
            }

            eventsList.Sort();
            foreach (var ev in eventsList)
            {
                NearbyFavoriteEvents.Add(ev);
            }
        }

        public static void FetchNearbyEvents()
        {
            FetcherLastFM.FetchAllNearbyEvents(); // this will stick them in the observable collection and notify the delegate as well.
        }
    }
}
