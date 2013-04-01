using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BandFunk
{
    public class SerializableVenue
    {
        public string Title { get; set; }
        public string PhoneNumber { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public Uri ImageUri { get; set; }

        public SerializableVenue() { }

        public SerializableVenue(Venue venue)
        {
            Title = venue.Title;
            PhoneNumber = venue.PhoneNumber;
            StreetAddress = venue.StreetAddress;
            City = venue.City;
            Country = venue.Country;
            Latitude = venue.Latitude;
            Longitude = venue.Longitude;
            ImageUri = venue.ImageUri;
        }

        public Venue ToVenue()
        {
            var venue = new Venue();
            venue.Title = Title;
            venue.PhoneNumber = PhoneNumber;
            venue.StreetAddress = StreetAddress;
            venue.City = City;
            venue.Country = Country;
            venue.Latitude = Latitude;
            venue.Longitude = Longitude;
            venue.ImageUri = ImageUri;
            return venue;
        }
    }
}
