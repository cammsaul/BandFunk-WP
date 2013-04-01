using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace BandFunk
{
    public static class ArtistFetcher
    {
        public delegate void ArtistFetchedDelegate(Artist artist);
        public static ArtistFetchedDelegate ArtistFetched;

        public delegate void ArtistPartiallyFetchedDelegate(Artist artist);
        public static ArtistPartiallyFetchedDelegate ArtistPartiallyFetched;

        static ArtistFetcher()
        {
            FetcherLastFM.ArtistFetched += artist =>
                {
                    if (artist == null)
                    {
                        ArtistFetched(null);
                        return;
                    }

                    ArtistFetcherEchoNest.FetchArtist(artist);
                };
            ArtistFetcherEchoNest.ArtistFetched += artist =>
                {
                    artist.LastFetchDate = DateTime.Now;
                    artist.IsFetchingEvents = true;                    
                    FetcherLastFM.FetchEventsForArtist(artist);
                    if (artist.RelatedArtistNames.Count == 0)
                    {
                        artist.IsFetchingSimilarArtists = true;
                        ArtistFetcherEchoNest.FetchSimilarArtists(artist);
                    }
                    if (ArtistPartiallyFetched != null)
                    {
                        ArtistPartiallyFetched(artist);
                    }
                    ArtistStore.Set(artist);
                };
            FetcherLastFM.EventsForArtistFetched += artist =>
                {
                    artist.IsFetchingEvents = false;
                    if (!artist.IsFetchingEvents && !artist.IsFetchingSimilarArtists)
                    {
                        if (ArtistFetched != null)
                        {
                            ArtistFetched(artist);
                        }
                    }
                };
            ArtistFetcherEchoNest.SimilarArtistsFetched += artist =>
                {
                    artist.IsFetchingSimilarArtists = false;
                    if (!artist.IsFetchingEvents && !artist.IsFetchingSimilarArtists)
                    {
                        if (ArtistFetched != null)
                        {
                            ArtistFetched(artist);
                        }
                    }
                };
        }

        public static void FetchArtist(string artistName, bool favorite = false)
        {
            FetcherLastFM.FetchArtist(artistName, favorite);            
        }

        public static void UpdateArtist(Artist artist)
        {
            Debug.WriteLine("Updating artist {0}...", artist.Name);
            artist.LastFetchDate = DateTime.Now;
            ArtistFetcherEchoNest.FetchArtist(artist); // will fetch events via LastFM when completed and then call ArtistFetchedDelegate
        }
    }
}
