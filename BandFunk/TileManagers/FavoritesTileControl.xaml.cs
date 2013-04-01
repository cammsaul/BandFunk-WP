using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;

namespace BandFunk
{
    public partial class FavoritesTileControl : UserControl
    {
        private const string SHARED_CONTENT_DIR = "/Shared/ShellContent/";
        private const string FilenameString = "/Shared/ShellContent/MainTile.jpg";
        private const string WideFilenameString = "/Shared/ShellContent/WideMainTile.jpg";
        public static Uri Filename { get; set; }
        public static Uri WideFilename { get; set; }

        private bool IsWP78 = false;
        private int HEIGHT { get { return IsWP78 ? 336 : 173; } }
        private bool IsWide = false;
        private int WIDTH { get { return IsWP78 ? (IsWide ? 672 : 336) : 173; } }

        static FavoritesTileControl()
        {
            Filename = new Uri("isostore:" + FilenameString, UriKind.Absolute);
            WideFilename = new Uri("isostore:" + WideFilenameString, UriKind.Absolute);
        }

        public static bool FileExists
        {
            get
            {
                using (var isolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    return isolatedStorage.DirectoryExists(SHARED_CONTENT_DIR) && isolatedStorage.FileExists(FilenameString);
                }
            }
        }

        public static bool WideFileExists
        {
            get
            {
                using (var isolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    return isolatedStorage.DirectoryExists(SHARED_CONTENT_DIR) && isolatedStorage.FileExists(WideFilenameString);
                }
            }
        }

        public FavoritesTileControl(Collection<Artist> favorites, bool isWP78, bool isWide)
        {
            InitializeComponent();
            IsWP78 = isWP78;
            IsWide = isWide;

            // set up the images
            int i = 0;
            foreach (var child in LayoutRoot.Children)
            {
                var image = child as Image;
                if (image != null && i < favorites.Count)
                {
                    var artist = favorites[i];
                    image.DataContext = artist;
                    i++;
                }
            }

            // layout the tile and save to disk
            Measure(new Size(WIDTH, HEIGHT));
            Arrange(new Rect(0, 0, WIDTH, HEIGHT));
            var bmp = new WriteableBitmap(WIDTH, HEIGHT);
            bmp.Render(this, null);
            bmp.Invalidate();

            using (var isolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!isolatedStorage.DirectoryExists(SHARED_CONTENT_DIR))
                {
                    isolatedStorage.CreateDirectory(SHARED_CONTENT_DIR);
                }

                var filename = IsWide ? WideFilenameString : FilenameString;
                using (var stream = isolatedStorage.OpenFile(filename, System.IO.FileMode.Create))
                {
                    bmp.SaveJpeg(stream, WIDTH, HEIGHT, 0, 100);
                }
            }
        }
    }
}
