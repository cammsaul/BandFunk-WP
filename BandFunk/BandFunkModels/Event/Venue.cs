using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace BandFunk
{
    public class Venue
    {
        public string Title { get; set; }
        public string PhoneNumber { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public BitmapImage Image { get; set; }
        public Uri ImageUri { get; set; }
    }
}
