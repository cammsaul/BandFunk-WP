using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BandFunk
{
    public static class WindowsPhone8Tiles
    {
        public class FlipTileData
        {
            public string Title { get; set; }
            public string BackTitle { get; set; }
            public string BackContent { get; set; }
            public string WideBackContent { get; set; }
            public int Count { get; set; }
            //public Uri TileId { get; set; }
            public Uri SmallBackgroundImage { get; set; }
            public Uri BackgroundImage { get; set; }
            public Uri BackBackgroundImage { get; set; }
            public Uri WideBackgroundImage { get; set; }
            public Uri WideBackBackgroundImage { get; set; }

            public void UpdateTile(ShellTile shellTile)
            {
                Type flipTileDataType = Type.GetType("Microsoft.Phone.Shell.FlipTileData, Microsoft.Phone");
                Type shellTileType = Type.GetType("Microsoft.Phone.Shell.ShellTile, Microsoft.Phone");
                var updateTileData = flipTileDataType.GetConstructor(new Type[] { }).Invoke(null);

                // Set the properties. 
                SetProperty(updateTileData, "Title", Title);
                SetProperty(updateTileData, "Count", Count);
                SetProperty(updateTileData, "BackTitle", BackTitle);
                SetProperty(updateTileData, "BackContent", BackContent);
                SetProperty(updateTileData, "SmallBackgroundImage", SmallBackgroundImage); // 159 x 159
                SetProperty(updateTileData, "BackgroundImage", BackgroundImage); // 336 x 336
                SetProperty(updateTileData, "BackBackgroundImage", BackBackgroundImage);
                SetProperty(updateTileData, "WideBackgroundImage", WideBackgroundImage);
                SetProperty(updateTileData, "WideBackBackgroundImage", WideBackBackgroundImage);
                SetProperty(updateTileData, "WideBackContent", WideBackContent);

                // Invoke the new version of ShellTile.Update.
                shellTileType.GetMethod("Update").Invoke(shellTile, new Object[] { updateTileData });
            }

            public void CreateTile(Uri uri, bool supportsWideTile)
            {
                Type flipTileDataType = Type.GetType("Microsoft.Phone.Shell.FlipTileData, Microsoft.Phone");
                Type shellTileType = Type.GetType("Microsoft.Phone.Shell.ShellTile, Microsoft.Phone");
                var newTileData = flipTileDataType.GetConstructor(new Type[] { }).Invoke(null);

                // Set the properties. 
                SetProperty(newTileData, "Title", Title);
                SetProperty(newTileData, "Count", Count);
                SetProperty(newTileData, "BackTitle", BackTitle);
                SetProperty(newTileData, "BackContent", BackContent);
                SetProperty(newTileData, "SmallBackgroundImage", SmallBackgroundImage); // 159 x 159
                SetProperty(newTileData, "BackgroundImage", BackgroundImage); // 336 x 336
                SetProperty(newTileData, "BackBackgroundImage", BackBackgroundImage);
                SetProperty(newTileData, "WideBackgroundImage", WideBackgroundImage);
                SetProperty(newTileData, "WideBackBackgroundImage", WideBackBackgroundImage);
                SetProperty(newTileData, "WideBackContent", WideBackContent);

                shellTileType.GetMethod("Create", new Type[] { typeof(Uri), typeof(ShellTileData), typeof(bool) }).Invoke(null, new Object[] { uri, newTileData, supportsWideTile });
            }
        }

        public class CycleTileData
        {
            public string Title { get; set; }
            public int Count { get; set; }
            //public Uri TileId { get; set; }
            public Uri[] CycleImages { get; set; } // one to nine images
            public Uri SmallBackgroundImage { get; set; }

            private Uri[] SaveImagesToDisk(Uri[] images)
            {
                var uris = new Uri[images.Count() >= 9 ? 9 : images.Count()];
                using (var isolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    var sharedContentDir = "/Shared/ShellContent/Genres/";
                    if (!isolatedStorage.DirectoryExists(sharedContentDir))
                    {
                        isolatedStorage.CreateDirectory(sharedContentDir);
                    }
                    for (int i = 0; i < images.Count() && i < 9; i++)
                    {
                        var remoteImage = images[i];
                        var filename = String.Format("{0}{1}.{2}.jpg", sharedContentDir, Title, i);
                        using (var stream = isolatedStorage.OpenFile(filename, System.IO.FileMode.OpenOrCreate))
                        {
                            var grid = new Grid();

                            var backgroundImage = new Image();
                            backgroundImage.Source = new BitmapImage(new Uri("/Images/Background.wide.withLogo.png", UriKind.Relative));
                            grid.Children.Add(backgroundImage);

                            var binding = new Binding("Uri");                            
                            var image = new Image();
                            image.Stretch = Stretch.UniformToFill;
                            image.SetBinding(Image.SourceProperty, binding);
                            image.Source = new BitmapImage(remoteImage);                 
                            grid.Children.Add(image);

                            grid.Measure(new Size(672, 336));
                            grid.Arrange(new Rect(0, 0, 672, 336));

                            var bmp = new WriteableBitmap(672, 336);
                            bmp.Render(grid, null);
                            bmp.Invalidate();
                            bmp.SaveJpeg(stream, 672, 336, 0, 100);
                        }
                        uris[i] = new Uri("isostore:" + filename, UriKind.Absolute);
                    }
                }
                return uris;
            }

            public void UpdateTile(ShellTile shellTile)
            {
                Type cycleTileDataType = Type.GetType("Microsoft.Phone.Shell.CycleTileData, Microsoft.Phone");
                Type shellTileType = Type.GetType("Microsoft.Phone.Shell.ShellTile, Microsoft.Phone");
                var updateTileData = cycleTileDataType.GetConstructor(new Type[] { }).Invoke(null);

                // Set the properties. 
                SetProperty(updateTileData, "Title", Title);
                SetProperty(updateTileData, "Count", Count);
                if (CycleImages == null || CycleImages.Count() < 1 || CycleImages.Count() > 9)
                {
                    throw new Exception("Invalid number of images in CycleImages array");
                }
                SetProperty(updateTileData, "CycleImages", SaveImagesToDisk(CycleImages));
                SetProperty(updateTileData, "SmallBackgroundImage", SmallBackgroundImage); // 159 x 159

                // Invoke the new version of ShellTile.Update.
                shellTileType.GetMethod("Update").Invoke(shellTile, new Object[] { updateTileData });
            }

            public void CreateTile(Uri uri, bool supportsWideTile)
            {
                Type cycleTileDataType = Type.GetType("Microsoft.Phone.Shell.CycleTileData, Microsoft.Phone");
                Type shellTileType = Type.GetType("Microsoft.Phone.Shell.ShellTile, Microsoft.Phone");
                var newTileData = cycleTileDataType.GetConstructor(new Type[] { }).Invoke(null);

                // Set the properties. 
                SetProperty(newTileData, "Title", Title);
                SetProperty(newTileData, "Count", Count);
                if (CycleImages == null || CycleImages.Count() < 1 || CycleImages.Count() > 9)
                {
                    throw new Exception("Invalid number of images in CycleImages array");
                }
                SetProperty(newTileData, "CycleImages", SaveImagesToDisk(CycleImages));
                SetProperty(newTileData, "SmallBackgroundImage", SmallBackgroundImage); // 159 x 159


                shellTileType.GetMethod("Create", new Type[] { typeof(Uri), typeof(ShellTileData), typeof(bool) }).Invoke(null, new Object[] { uri, newTileData, supportsWideTile });
            }
        }

        private static void SetProperty(object instance, string name, object value)
        {
            var setMethod = instance.GetType().GetProperty(name).GetSetMethod();
            setMethod.Invoke(instance, new object[] { value });
        }
    }
}
