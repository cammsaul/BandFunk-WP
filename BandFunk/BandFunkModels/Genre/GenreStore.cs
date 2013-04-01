using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace BandFunk
{
    public static class GenreStore
    {
        private static Dictionary<string, Genre> Genres { get; set; }

        public delegate void GenrePartiallyFetchedDelegate(Genre genre);
        public static GenrePartiallyFetchedDelegate GenrePartiallyFetched;

        public delegate void GenreFetchedDelegate(Genre genre);
        public static GenreFetchedDelegate GenreFetched;

        private const string API_KEY = "7e608454ab11cae9d75726ea6258ca40";

        static GenreStore()
        {
            Genres = new Dictionary<string, Genre>();
        }

        public static Genre Get(string genreName)
        {
            return Genres.ContainsKey(genreName.ToLower()) ? Genres[genreName.ToLower()] : null;
        }

        public static void Set(Genre genre)
        {
            if (Genres.ContainsKey(genre.Name.ToLower()))
            {
                Genres[genre.Name.ToLower()] = genre;
            }
            else
            {
                Genres.Add(genre.Name.ToLower(), genre);
            }
        }

        public static void FetchGenre(string genreName)
        {
            var request = String.Format("http://ws.audioscrobbler.com/2.0/?method=tag.getInfo&tag={0}&api_key={1}&format=json", Uri.EscapeDataString(genreName.ToLower()), API_KEY);
            Debug.WriteLine("LastFM: Fetching Genre {0}: {1}", genreName, request);
            var webClient = new WebClient();
            webClient.DownloadStringCompleted += webClient_FetchGenreInfo_DownloadStringCompleted;
            webClient.DownloadStringAsync(new Uri(request, UriKind.Absolute));
        }

        private static void webClient_FetchGenreInfo_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                var result = e.Result;
            }
            catch
            {
                Debug.WriteLine("Unable to fetch genre (no internet connection?)");
                if (GenrePartiallyFetched != null)
                {
                    GenrePartiallyFetched(null);
                }
                if (GenreFetched != null)
                {
                    GenreFetched(null);
                }
                return;
            }

            JObject json = JObject.Parse(e.Result);
            var tagJson = json["tag"];

            var genre = new Genre();
            genre.Name = tagJson["name"].Value<string>();
            genre.LastFMUri = new Uri(tagJson["url"].Value<string>());
            try
            {
                genre.Description = Utility.ClearHTMLTagsFromString(HttpUtility.HtmlDecode(tagJson["wiki"]["content"].Value<string>()));
            }
            catch { }

            GenreStore.Set(genre);
            if (GenrePartiallyFetched != null)
            {
                GenrePartiallyFetched(genre);
            }

            // now fetch top artists
            var request = String.Format("http://ws.audioscrobbler.com/2.0/?method=tag.getTopArtists&tag={0}&api_key={1}&format=json", Uri.EscapeDataString(genre.Name.ToLower()), API_KEY);
            Debug.WriteLine("LastFM: Fetching Genre Top Artists {0}: {1}", genre.Name, request);
            var webClient = new WebClient();
            var userToken = new Utility.WebClientToken();
            userToken.Genre = genre;
            webClient.DownloadStringCompleted += webClient_FetchGenreTopArtists_DownloadStringCompleted;
            webClient.DownloadStringAsync(new Uri(request, UriKind.Absolute), userToken);
        }

        private static void webClient_FetchGenreTopArtists_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            JObject json = JObject.Parse(e.Result);
            var genre = (e.UserState as Utility.WebClientToken).Genre;

            foreach (var artistJson in json["topartists"]["artist"].Children())
            {
                var artist = new Genre.ArtistNameAndImage();
                artist.Name = artistJson["name"].Value<string>().ToLower();
                foreach (var imgJson in artistJson["image"].Children())
                {
                    if (imgJson["size"].Value<string>() == "extralarge")
                    {
                        artist.ImageUri = new Uri(imgJson["#text"].Value<string>());
                        artist.Image = new BitmapImage(artist.ImageUri);
                        break;
                    }
                }
                genre.TopArtists.Add(artist);
            }

            // now fetch related genres
            var request = String.Format("http://ws.audioscrobbler.com/2.0/?method=tag.getSimilar&tag={0}&api_key={1}&format=json", Uri.EscapeDataString(genre.Name.ToLower()), API_KEY);
            Debug.WriteLine("LastFM: Fetching Similar Genres {0}: {1}", genre.Name, request);
            var webClient = new WebClient();
            var userToken = new Utility.WebClientToken();
            userToken.Genre = genre;
            webClient.DownloadStringCompleted += webClient_FetchSimilarGenres_DownloadStringCompleted;
            webClient.DownloadStringAsync(new Uri(request, UriKind.Absolute), userToken);
        }

        private static void webClient_FetchSimilarGenres_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            JObject json = JObject.Parse(e.Result);
            var genre = (e.UserState as Utility.WebClientToken).Genre;

            foreach (var genreJson in json["similartags"]["tag"].Children())
            {
                genre.SimilarGenres.Add(genreJson["name"].Value<string>());
            }

            Debug.WriteLine("LastFM: Finished Fetching Genre {0}.", genre.Name);
            if (GenreFetched != null)
            {
                GenreFetched(genre);
            }
        }
    }
}
