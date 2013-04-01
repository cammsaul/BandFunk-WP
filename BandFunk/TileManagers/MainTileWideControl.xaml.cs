using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;

namespace BandFunk
{
    public partial class MainTileWideControl : UserControl
    {
        private const string SHARED_CONTENT_DIR = "/Shared/ShellContent/";
        private const string FILENAME = "/Shared/ShellContent/WideMainTileBack.jpg";
        public static Uri Filename { get; set; }

        private const int HEIGHT = 336;
        private const int WIDTH = 672;

        static MainTileWideControl()
        {
            Filename = new Uri("isostore:" + FILENAME, UriKind.Absolute);
        }

        public static bool FileExists
        {
            get
            {
                using (var isolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    return isolatedStorage.DirectoryExists(SHARED_CONTENT_DIR) && isolatedStorage.FileExists(FILENAME);
                }
            }
        }

        public MainTileWideControl(Event ev = null)
        {
            InitializeComponent();

            using (var isolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                DataContext = ev;

                if (!isolatedStorage.DirectoryExists(SHARED_CONTENT_DIR))
                {
                    isolatedStorage.CreateDirectory(SHARED_CONTENT_DIR);
                }
                using (var stream = isolatedStorage.OpenFile(FILENAME, System.IO.FileMode.OpenOrCreate))
                {
                    Measure(new Size(672, 336));
                    Arrange(new Rect(0, 0, 672, 336));

                    var bmp = new WriteableBitmap(672, 336);
                    bmp.Render(this, null);
                    bmp.Invalidate();
                    bmp.SaveJpeg(stream, 672, 336, 0, 100);
                }
            }
        }
    }
}