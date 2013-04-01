using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;

namespace BandFunk
{
    public partial class GenreTileControl : UserControl
    {
        public string BackContent { get; set; }
        public Uri Filename { get; set; }

        private bool IsTiny = false;
        private const int REGULAR_SIZE = 173;
        private const int TINY_SIZE = 159;
        private int Size { get { return IsTiny ? TINY_SIZE : REGULAR_SIZE; } } 

        public GenreTileControl(Genre genre, bool isTiny = false)
        {
            InitializeComponent();

            IsTiny = isTiny;

            // set up the images
            int i = 0;
            foreach (var child in LayoutRoot.Children)
            {
                var image = child as Image;
                if (image != null && i < genre.TopArtists.Count)
                {
                    var topArtist = genre.TopArtists[i];
                    image.DataContext = topArtist;
                    i++;
                }
            }

            // create the back content
            BackContent = String.Empty;
            for (i = 0; genre.TopArtists != null && i < 5 && i < genre.TopArtists.Count; i++)
            {
                BackContent = String.Format("{0}{1}\n", BackContent, genre.TopArtists[i].Name);
            }

            // layout the tile and save to disk
            Measure(new Size(Size, Size));
            Arrange(new Rect(0, 0, Size, Size));
            var bmp = new WriteableBitmap(Size, Size);
            bmp.Render(this, null);
            bmp.Invalidate();

            String filename = null;
            using (var isolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                var sharedContentDir = "/Shared/ShellContent/";
                if (IsTiny)
                {
                    filename = sharedContentDir + genre.Name + ".tiny.jpg";
                }
                else
                {
                    filename = sharedContentDir + genre.Name + ".jpg";
                }

                if (!isolatedStorage.DirectoryExists(sharedContentDir))
                {
                    isolatedStorage.CreateDirectory(sharedContentDir);
                }

                using (var stream = isolatedStorage.OpenFile(filename, System.IO.FileMode.OpenOrCreate))
                {
                    bmp.SaveJpeg(stream, Size, Size, 0, 100);
                }
            }

            Filename = new Uri("isostore:" + filename, UriKind.Absolute);
        }
    }
}
