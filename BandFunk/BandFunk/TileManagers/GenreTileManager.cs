using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BandFunk
{
    public static class GenreTileManager
    {
        public static void PinToStart(Genre genre)
        {
            var uri = new Uri(String.Format("/Views+Pages/GenrePage.xaml?genre={0}", Uri.EscapeDataString(genre.Name.ToLower())), UriKind.Relative);
            if (Utility.IsWindowsPhone7_8OrHigher)
            {
                var smallTile = new GenreTileControl(genre, isTiny:true);

                var images = new Uri[genre.TopArtists.Count > 0 ? 9 : genre.TopArtists.Count];
                for (int i = 0; i < 9 && i < genre.TopArtists.Count; i++)
                {
                    images[i] = genre.TopArtists[i].ImageUri;
                }
                var newTile = new WindowsPhone8Tiles.CycleTileData()
                {
                    Title = Utility.TitleCaseString(genre.Name),
                    CycleImages = images,
                    SmallBackgroundImage = smallTile.Filename
                };
                newTile.CreateTile(uri, supportsWideTile:true);                
            }
            else
            {
                var genreTile = new GenreTileControl(genre);            
                var newTile = new Microsoft.Phone.Shell.StandardTileData()
                {
                    Title = Utility.TitleCaseString(genre.Name),
                    BackgroundImage = genreTile.Filename,
                    BackBackgroundImage = new Uri("/Images/blank_tile.png", UriKind.Relative),
                    BackTitle = Utility.TitleCaseString(genre.Name),
                    BackContent = genreTile.BackContent
                };
                ShellTile.Create(uri, newTile);
            }                       
        }

        public static bool EnableAddLiveTile(Genre genre)
        {
            var tileUri = new Uri(String.Format("/Views+Pages/GenrePage.xaml?genre={0}", Uri.EscapeDataString(genre.Name.ToLower())), UriKind.Relative);
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
