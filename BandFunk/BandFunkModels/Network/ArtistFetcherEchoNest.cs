using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;

namespace BandFunk
{
    public static class ArtistFetcherEchoNest
    {

        private const string API_KEY = "IBUWKCWDXHQWP5WIV";

        public delegate void ArtistFetchedDelegate(Artist artist);
        public static ArtistFetchedDelegate ArtistFetched;

        public delegate void SimilarArtistsFetchedDelegate(Artist artist);
        public static SimilarArtistsFetchedDelegate SimilarArtistsFetched;        

        /// <summary>
        /// Fetches News and EchoNestArtistID for an Artist.
        /// </summary>
        public static void FetchArtist(Artist artist)
        {
            var request = String.Format("http://developer.echonest.com/api/v4/artist/profile?api_key={0}&format=json&name={1}&bucket=news",
                API_KEY, Uri.EscapeDataString(artist.Name));
            Debug.WriteLine("EchoNest: Fetching Artist {0}: {1}", artist.Name, request);
            var webClient = new WebClient();
            var token = new Utility.WebClientToken();
            token.Artist = artist;
            webClient.DownloadStringCompleted += webClient_FetchArtist_DownloadStringCompleted;
            webClient.DownloadStringAsync(new Uri(request, UriKind.Absolute), token);
        }

        private static void webClient_FetchArtist_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            var token = (e.UserState as Utility.WebClientToken);
            var artist = token.Artist;
            
            JObject json = JObject.Parse(e.Result);
            var newsJsonArray = json["response"]["artist"]["news"];
            foreach (var newsJson in newsJsonArray.Children())
            {
                artist.NewsItems.Clear();
                var newsItem = new NewsItem()
                {
                    Name = newsJson["name"].Value<string>(),
                    Uri = new Uri(newsJson["url"].Value<string>()),
                    Summary = Utility.ClearHTMLTagsFromString(HttpUtility.HtmlDecode(newsJson["summary"].Value<string>()))
                };
                artist.NewsItems.Add(newsItem);
                artist.EchoNestArtistID = json["response"]["artist"]["id"].Value<string>();
            }

            Debug.WriteLine("EchoNest: Fetched Artist {0}", artist.Name);
            ArtistFetched(artist);
        }

        public static void FetchSimilarArtists(Artist artist)
        {
            if (artist.EchoNestArtistID == null)
            {
                Debug.WriteLine("EchoNest: Artist '{0}' does not have EchoNest ID. Can't fetch similar artists!", artist.Name);
                SimilarArtistsFetched(artist);
                return;
            }

            var request = String.Format("http://developer.echonest.com/api/v4/artist/similar?api_key={0}&id={1}&format=json&start=0", API_KEY, artist.EchoNestArtistID);
            Debug.WriteLine("EchoNest: Fetching Similar Artists for {0}: {1}", artist.Name, request);
            var webClient = new WebClient();
            var userToken = new Utility.WebClientToken();
            userToken.Artist = artist;
            webClient.DownloadStringCompleted += webClient_FetchSimilarArtists_DownloadStringCompleted;
            webClient.DownloadStringAsync(new Uri(request), userToken);
        }

        private static void webClient_FetchSimilarArtists_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            var artist = (e.UserState as Utility.WebClientToken).Artist;

            JObject json = JObject.Parse(e.Result);
            foreach (var artistJson in json["response"]["artists"].Children())
            {
                artist.RelatedArtistNames.Add(artistJson["name"].Value<string>());
            }

            Debug.WriteLine("LastFM: Fetched Similar Artists for {0}", artist.Name);
            SimilarArtistsFetched(artist);
        }

    }
}
