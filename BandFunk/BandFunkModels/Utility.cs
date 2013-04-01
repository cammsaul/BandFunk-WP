using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace BandFunk
{
    public static class Utility
    {
        public static string ClearHTMLTagsFromString(string htmlString)
        {
            string regEx = @"\<[^\<\>]*\>";
            string tagless = Regex.Replace(htmlString, regEx, string.Empty);

            // remove rogue leftovers
            tagless = tagless.Replace("<", string.Empty).Replace(">", string.Empty);

            return tagless;
        }

        public class WebClientToken
        {
            public Artist Artist;
            public Genre Genre;
            public bool MakeFavorite;
            public string Name;
        }

        /// <summary>
        /// This is in Miles
        /// </summary>
        public static double DistanceBetweenCoordinates(double startLatitude, double startLongitude, double endLatitude, double endLongitude)
        {
            const int RADIUS = 6371000; // Earth's radius in meters
            const double RAD_PER_DEG = 0.017453293;

            double dlat = endLatitude - startLatitude;
            double dlon = endLongitude - startLongitude;

            double dlon_rad = dlon * RAD_PER_DEG;
            double dlat_rad = dlat * RAD_PER_DEG;
            double lat1_rad = startLatitude * RAD_PER_DEG;
            double lon1_rad = startLongitude * RAD_PER_DEG;
            double lat2_rad = endLatitude * RAD_PER_DEG;
            double lon2_rad = endLongitude * RAD_PER_DEG;

            double a = Math.Pow(Math.Sin(dlat_rad / 2), 2) + Math.Cos(lat1_rad) * Math.Cos(lat2_rad) * Math.Pow(Math.Sin(dlon_rad / 2), 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double d = RADIUS * c;

            return d * 0.000621371192; // convert to miles
        }

        public static double DistanceBetweenCoordinates(GeoCoordinate coordinate1, GeoCoordinate coordinate2)
        {
            return DistanceBetweenCoordinates(coordinate1.Latitude, coordinate1.Longitude, coordinate2.Latitude, coordinate2.Longitude);
        }

        /// <summary>
        /// Converts to title case: each word starts with an upper case.
        /// </summary>
        public static string TitleCaseString(string value)
        {
            if (value == null)
                return null;
            if (value.Length == 0)
                return value;

            StringBuilder result = new StringBuilder(value);
            result[0] = char.ToUpper(result[0]);
            for (int i = 1; i < result.Length; ++i)
            {
                if (char.IsWhiteSpace(result[i - 1]))
                    result[i] = char.ToUpper(result[i]);
                else
                    result[i] = char.ToLower(result[i]);
            }
            return result.ToString();
        }

        private static Version VERSION_7_8 = new Version(7, 10, 8858);
        private static Version VERSION_8 = new Version(8, 0);

        public static bool IsTargetedVersion(Version targetedVersion)
        {
            return Environment.OSVersion.Version >= targetedVersion;
        }
        

        public static bool IsWindowsPhone7_8OrHigher
        {
            get
            {
                return IsTargetedVersion(VERSION_7_8);
            }
        }

        public static bool IsWindowsPhone8OrHigher
        {
            get
            {
                return IsTargetedVersion(VERSION_8);
            }
        }
    }
}
