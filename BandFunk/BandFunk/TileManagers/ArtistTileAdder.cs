using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BandFunk
{
    public static class ArtistTileAdder
    {
        /// <summary>
        /// Pins an artist to the start. Assumes that this action is valid (e.g. tile does not already exist)
        /// </summary>
        public static void PinArtistToStart(Artist artist)
        {
            var uri = new Uri("/Views+Pages/ArtistPivotPage.xaml?artistName=" + Uri.EscapeDataString(artist.Name), UriKind.Relative);
            if (Utility.IsWindowsPhone7_8OrHigher)
            {
                var newTile = new WindowsPhone8Tiles.FlipTileData()
                {
                    Title = artist.CapitalizedName,
                    BackgroundImage = artist.TileImageUri,
                    BackTitle = artist.LiveTileBackText != null ? artist.CapitalizedName : null,
                    BackContent = artist.LiveTileBackText,
                    BackBackgroundImage = artist.LiveTileBackText != null ? new Uri("/Images/blank_tile.png", UriKind.Relative) : null,
                    WideBackgroundImage = artist.MainImageUri,
                    WideBackContent = artist.LiveTileBackText,
                    WideBackBackgroundImage = artist.LiveTileBackText != null ? new Uri("/Images/Background.wide.png", UriKind.Relative) : null
                };
                newTile.CreateTile(uri, supportsWideTile: true);
                //2011: The background agent can’t use Microsoft.Phone.Shell.ShellTile::Create, which assembly TileManagers.dll is trying to use. Update your file and then try again.
            }
            else
            {

                var newTile = new StandardTileData()
                {
                    Title = artist.CapitalizedName,
                    BackgroundImage = artist.TileImageUri,
                    BackTitle = artist.LiveTileBackText != null ? artist.CapitalizedName : null,
                    BackContent = artist.LiveTileBackText,
                    BackBackgroundImage = artist.LiveTileBackText != null ? new Uri("/Images/blank_tile.png", UriKind.Relative) : null
                };
                ShellTile.Create(uri, newTile);
            }
        }
    }
}
