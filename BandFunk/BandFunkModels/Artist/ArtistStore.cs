using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;

namespace BandFunk
{
    public class ArtistStore
    {

        private static Dictionary<string, Artist> Artists { get; set; }
        public static ObservableCollection<Artist> Favorites { get; private set; }

        static ArtistStore()
        {
            Favorites = new ObservableCollection<Artist>();
            Artists = new Dictionary<string, Artist>();
        }
        
        public static void Set(Artist artist)
        {
            if (!Artists.ContainsKey(artist.Name.ToLower()))
            {
                Artists.Add(artist.Name.ToLower(), artist);
            }
            else
            {
                Artists[artist.Name.ToLower()] = artist;
            }
        }

        public static Artist Get(string artistName)
        {
            return Artists.ContainsKey(artistName.ToLower()) ? Artists[artistName.ToLower()] : null;
        }

        public static List<Artist> SortedFavorites
        {
            get
            {
                var list = Favorites.ToList();
                list.Sort();
                return list;
            }
        }

        public static void Save()
        {
            SerializableArtist[] artistArray = new SerializableArtist[Artists.Count];
            int i = 0;
            foreach (var artist in Artists.Values)
            {
                artistArray[i] = new SerializableArtist(artist);
                i++;
            }

            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            using (var file = store.CreateFile("ArtistStore.json"))
            {
                // create the serializer for the class
                var serializer = new DataContractJsonSerializer(typeof(SerializableArtist[]));

                // save the object as json
                serializer.WriteObject(file, artistArray);
            }
            Debug.WriteLine("Saved ArtistStore. ({0} artists, {1} favorites)", Artists.Count, Favorites.Count);
        }

        public static bool Restore()
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.FileExists("ArtistStore.json"))
                {
                    using (var file = store.OpenFile("ArtistStore.json", FileMode.Open))
                    {
                        // create the serializer
                        var serializer = new DataContractJsonSerializer(typeof(SerializableArtist[]));

                        // load the object from JSON
                        SerializableArtist[] artistArray = (SerializableArtist[])serializer.ReadObject(file);
                        Artists = new Dictionary<string, Artist>();

                        if (artistArray != null)
                        {
                            foreach (var serializableArtist in artistArray)
                            {
                                var artist = serializableArtist.ToArtist();
                                Set(artist);
                                if (serializableArtist.IsFavorite)
                                {
                                    Favorites.Add(artist);
                                }

                                var eventsToRemove = new List<Event>();

                                // add all of the artists events to the event store if needed. Trash anything that happened in the past
                                foreach (var ev in artist.Events)
                                {
                                    if (DateTime.Now > ev.Date)
                                    {
                                        eventsToRemove.Add(ev);
                                    }
                                    else if (!EventStore.Events.ContainsKey(ev.Id))
                                    {
                                        EventStore.Events.Add(ev.Id, ev);
                                    }
                                }

                                // remove old events
                                foreach (var ev in eventsToRemove)
                                {
                                    artist.Events.Remove(ev);
                                }
                            }
                        }
                        Debug.WriteLine("Restored ArtistStore: {0} artists ({1} favorites).", (artistArray != null ? artistArray.Count() : 0), Favorites.Count);
                        return true;
                    }
                }
            }
            return false;
        }

        public static void UpdateFavoritesIfNeeded()
        {
            foreach (var favorite in Favorites)
            {
                if (favorite.LastFetchDate == null || ((DateTime.Now - favorite.LastFetchDate) > TimeSpan.FromDays(7)))
                {
                    Debug.WriteLine("Artist {0} out of date (last fetch was {1}). Updating.", favorite.Name, (favorite.LastFetchDate != null ? (DateTime.Now - favorite.LastFetchDate).Days.ToString() + " days" : "never"));
                    ArtistFetcher.UpdateArtist(favorite);
                }
                else
                {
                    Debug.WriteLine("Artist {0} is not out of date (last fetch was {1} days ago).", favorite.Name, (DateTime.Now - favorite.LastFetchDate).Days.ToString());
                }
            }
        }
    }
}
