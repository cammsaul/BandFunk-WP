using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TileManagers;

namespace BandFunk
{
    public static class MainTileUpdater
    {
        /// <summary>
        /// We only want to update the background images if they don't exist or if we're leaving the main page. If we update them otherwise, the images woun't be fetched
        /// from the web and we'll have a blank flip side.
        /// </summary>
        /// <param name="updateBackgroundImages"></param>
        public static void UpdateMainTile(bool forceUpdateBackgroundImages)
        {
            var isWP78 = Utility.IsWindowsPhone7_8OrHigher;
            if (forceUpdateBackgroundImages || !FavoritesTileControl.FileExists)
            {
                var favoritesTile = new FavoritesTileControl(ArtistStore.Favorites, isWP78: isWP78, isWide: false);
            }
            var mainTile = ShellTile.ActiveTiles.First();
            if (!isWP78)
            {
                var tileData = new StandardTileData()
                {
                    Title = "BandFunk",
                    BackgroundImage = new Uri("/Background.png", UriKind.Relative),
                    BackTitle = "BandFunk",
                    BackBackgroundImage = FavoritesTileControl.Filename
                };
                mainTile.Update(tileData);
            }
            else
            {
                // render the controls for the wide tile
                if (forceUpdateBackgroundImages || !FavoritesTileControl.FileExists || !FavoritesTileControl.WideFileExists)
                {
                    var favoritesTile = new FavoritesTileControl(ArtistStore.Favorites, isWP78: isWP78, isWide: false);
                    var wideFavoritesTile = new FavoritesTileControl(ArtistStore.Favorites, isWP78: isWP78, isWide: true);
                }

                // always update the front tile (new event etc)
                Event nearbyFavorite = EventStore.NearbyFavoriteEvents.Count > 0 ? EventStore.NearbyFavoriteEvents[0] : null;
                var wideFrontTile = new MainTileWideControl(nearbyFavorite);

                var tileData = new WindowsPhone8Tiles.FlipTileData()
                {
                    Title = "BandFunk",
                    BackgroundImage = new Uri("/Images/Background.large.png", UriKind.Relative),
                    BackTitle = "BandFunk",
                    BackBackgroundImage = FavoritesTileControl.Filename,
                    SmallBackgroundImage = new Uri("/Images/Background.tiny.png", UriKind.Relative),
                    WideBackgroundImage = MainTileWideControl.Filename,
                    WideBackBackgroundImage = FavoritesTileControl.WideFilename,
                };
                tileData.UpdateTile(mainTile);
            }
            Debug.WriteLine(String.Format("Updated main tile. Updated images:{0}", forceUpdateBackgroundImages));
        }
    }
}
