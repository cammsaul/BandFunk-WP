using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Navigation;

namespace BandFunk
{
    public static class ArtistTileUpdater
    {
        /// <summary>
        /// Just looks for all current artist tiles and updates the back side with a short blurb about any nearby shows that might exist
        /// </summary>
        /// <param name="specificArtist">You can optionally specify a specifc artist to update only their tile. Otherwise (by default) it will update all artist tiles. </param>
        public static void UpdateArtistTiles(Artist specificArtist = null)
        {
            //var tileUri = new Uri("/Views+Pages/ArtistPivotPage.xaml?artistName=" + Uri.EscapeDataString(Name), UriKind.Relative);
            foreach (var tile in ShellTile.ActiveTiles.Where(t => t.NavigationUri.OriginalString.Contains("ArtistPivotPage.xaml")))
            {
                var artistName = Uri.UnescapeDataString(Regex.Replace(tile.NavigationUri.ToString(), "(?m)^.*artistName=(.*)$", "$1"));
                var artist = ArtistStore.Get(artistName);
                if (specificArtist == null || artist == specificArtist)
                {
                    if (artist != null)
                    {
                        if (Utility.IsWindowsPhone7_8OrHigher)
                        {
                            var tileData = new WindowsPhone8Tiles.FlipTileData()
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
                            tileData.UpdateTile(tile);
                        }
                        else
                        {

                            var tileData = new StandardTileData()
                            {
                                Title = artist.CapitalizedName,
                                BackgroundImage = artist.TileImageUri,
                                BackTitle = artist.LiveTileBackText != null ? artist.CapitalizedName : null,
                                BackContent = artist.LiveTileBackText,
                                BackBackgroundImage = artist.LiveTileBackText != null ? new Uri("/Images/blank_tile.png", UriKind.Relative) : null
                            };
                            tile.Update(tileData);
                        }
                    }
                }
            }
        }

        public static bool EnableAddLiveTile(Artist artist)
        {
            var tileUri = new Uri("/Views+Pages/ArtistPivotPage.xaml?artistName=" + Uri.EscapeDataString(artist.Name), UriKind.Relative);
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
