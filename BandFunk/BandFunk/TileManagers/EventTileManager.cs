using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BandFunk
{
    public static class EventTileManager
    {
        public static void PinToStart(Event anEvent)
        {
            var uri = new Uri(String.Format("/Views+Pages/EventPage.xaml?eventID={0}", anEvent.Id), UriKind.Relative);
            if (Utility.IsWindowsPhone7_8OrHigher)
            {
                var newTile = new WindowsPhone8Tiles.FlipTileData()
                {
                    Title = String.Format("{0} {1}", anEvent.Title, anEvent.Date.ToShortDateString()),
                    BackgroundImage = anEvent.ImageUri,
                    BackBackgroundImage = anEvent.Venue.ImageUri,
                    BackTitle = anEvent.Venue.Title,
                    BackContent = anEvent.Date.ToShortDateString(),
                    WideBackgroundImage = anEvent.ImageUri,
                    WideBackContent = String.Format("{0}\n{1}\n{2}", anEvent.FormattedDateString, anEvent.Venue.StreetAddress, anEvent.Venue.City),
                    WideBackBackgroundImage = anEvent.Venue.ImageUri
                };
                newTile.CreateTile(uri, supportsWideTile: true);
            }
            else
            {

                var newTile = new StandardTileData()
                {
                    Title = String.Format("{0} {1}", anEvent.Title, anEvent.Date.ToShortDateString()),
                    BackgroundImage = anEvent.ImageUri,
                    BackBackgroundImage = anEvent.Venue.ImageUri,
                    BackTitle = anEvent.Venue.Title,
                    BackContent = anEvent.Date.ToShortDateString()
                };
                ShellTile.Create(uri, newTile);
            }
        }

        public static bool EnableAddLiveTile(Event e)
        {
            var tileUri = new Uri(String.Format("/Views+Pages/EventPage.xaml?eventID={0}", e.Id), UriKind.Relative);
            foreach (var existingTile in ShellTile.ActiveTiles)
            {
                if (existingTile.NavigationUri == tileUri)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
