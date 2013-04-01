using System.Windows;
using Microsoft.Phone.Scheduler;
using System.Device.Location;
using System.Diagnostics;
using System;
using Microsoft.Phone.Shell;

namespace BandFunk
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        private static volatile bool _classInitialized;

        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        public ScheduledAgent()
        {
            if (!_classInitialized)
            {
                _classInitialized = true;
                // Subscribe to the managed exception handler
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    Application.Current.UnhandledException += ScheduledAgent_UnhandledException;
                });
            }
        }

        /// Code to execute on Unhandled Exceptions
        private void ScheduledAgent_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        /// <summary>
        /// Agent that runs a scheduled task
        /// </summary>
        /// <param name="task">
        /// The invoked task
        /// </param>
        /// <remarks>
        /// This method is called when a periodic or resource intensive task is invoked
        /// </remarks>
        protected override void OnInvoke(ScheduledTask task)
        {
            Debug.WriteLine("Scheduled agent invoked.");
            try
            {
                ArtistStore.Restore();
            }
            catch
            {
                NotifyComplete(); // if we can't restore the artist store for some reason (the app is launching?) then we're done
                return;
            }

            // update main tile
            MainTileUpdater.UpdateMainTile(forceUpdateBackgroundImages: false);

            // find the oldest artist
            Artist oldestArtist = null;
            foreach (var favorite in ArtistStore.Favorites)
            {
                if ((DateTime.Now - favorite.LastFetchDate).Days > 1)
                {
                    if (oldestArtist == null || oldestArtist.LastFetchDate > favorite.LastFetchDate)
                    {
                        oldestArtist = favorite;
                    }
                }
            }

            if (oldestArtist == null)
            {
                NotifyComplete();
            }
            else
            {
                ArtistFetcher.UpdateArtist(oldestArtist);
            }

            ArtistFetcher.ArtistFetched += artist =>
            {
                var currentLocation = CurrentLocationManager.CurrentLocation;
                if (currentLocation == null)
                {
                    NotifyComplete();
                    return;
                }

                if (artist.NewEvents.Count > 0)
                {
                    foreach (var newEvent in artist.NewEvents)
                    {
                        if (newEvent.Venue.Latitude != null && newEvent.Venue.Longitude != null)
                        {
                            if (Utility.DistanceBetweenCoordinates(currentLocation.Latitude, currentLocation.Longitude, newEvent.Venue.Latitude.Value, 
                                newEvent.Venue.Longitude.Value) < CurrentLocationManager.NEARBY_DIST_THRESHOLD_MILES)
                            {
                                ShellToast popupMessage = new ShellToast()
                                {
                                    Title = "BandFunk",
                                    Content = "New nearby show for " + Utility.TitleCaseString(artist.Name) + "!",
                                    NavigationUri = new Uri(String.Format("/Views+Pages/EventPage.xaml?eventID={0}", newEvent.Id), UriKind.Relative)
                                };
                                popupMessage.Show();
                            }
                        }
                    }
                }

                ArtistStore.Save();
                ArtistTileUpdater.UpdateArtistTiles(artist);

                NotifyComplete();
            };
        }
    }
}