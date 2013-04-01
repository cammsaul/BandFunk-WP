using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace BandFunk
{
    public class Genre
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Uri LastFMUri { get; set; }
        public ObservableCollection<ArtistNameAndImage> TopArtists { get; set; }
        public ObservableCollection<string> SimilarGenres { get; set; }

        public Genre()
        {
            TopArtists = new ObservableCollection<ArtistNameAndImage>();
            SimilarGenres = new ObservableCollection<string>();
        }

        public class ArtistNameAndImage
        {
            public string Name { get; set; }
            public BitmapImage Image { get; set; }
            public Uri ImageUri { get; set; }
        }

        public string UppercaseName
        {
            get
            {
                return Name.ToUpper();
            }
        }
    }
}
