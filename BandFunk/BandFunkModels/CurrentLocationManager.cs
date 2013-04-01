using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Device.Location;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Text;

namespace BandFunk
{
    public static class CurrentLocationManager
    {
        public delegate void CurrentLocationChangedDelegate();
        public static CurrentLocationChangedDelegate CurrentLocationChanged;

        public const int NEARBY_DIST_THRESHOLD_MILES = 100;
        
        private const string CURRENT_LOCATION_KEY = "CurrentLocation";
        private const string CURRENT_LOCATION_NAME_KEY = "CurrentLocationName";
        private const string ENABLE_CURRENT_LOCATION_KEY = "EnableCurrentLocation";

        public static bool EnableCurrentLocation
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                if (settings.Contains(ENABLE_CURRENT_LOCATION_KEY))
                {
                    return (bool)settings[ENABLE_CURRENT_LOCATION_KEY];
                }
                else
                {
                    settings[ENABLE_CURRENT_LOCATION_KEY] = true;
                    return true;
                }
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings[ENABLE_CURRENT_LOCATION_KEY] = value;
                if (value == false)
                {
                    CurrentLocationName = "DISABLED";
                    CurrentLocation = null;
                }
                settings.Save();
            }
        }

        public static string CurrentLocationName
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                if (settings.Contains(CURRENT_LOCATION_NAME_KEY))
                {
                    return settings[CURRENT_LOCATION_NAME_KEY] as string;
                }
                return "NOT AVAILABLE"; // if they won't enable location services they'll have to be in New York >:(
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings[CURRENT_LOCATION_NAME_KEY] = value;
                settings.Save();
                if (CurrentLocationChanged != null)
                {
                    CurrentLocationChanged();
                }
            }
        }

        public static GeoCoordinate CurrentLocation
        {
            get
            {
                // grab current location from app setttings
                var settings = IsolatedStorageSettings.ApplicationSettings;
                if (settings.Contains(CURRENT_LOCATION_KEY))
                {
                    return settings[CURRENT_LOCATION_KEY] as GeoCoordinate;
                }
                return null;
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings[CURRENT_LOCATION_KEY] = value;
                settings.Save();
                if (CurrentLocationChanged != null)
                {
                    CurrentLocationChanged();
                }
            }
        }

        public static void UpdateCurrentLocation()
        {
            Debug.WriteLine("Updating current location...");
            if (!EnableCurrentLocation)
            {
                throw new Exception("Location services must be enabled.");
            }

            var geoCoordinateWatcher = new GeoCoordinateWatcher(GeoPositionAccuracy.Default);
            geoCoordinateWatcher.Start();
            geoCoordinateWatcher.PositionChanged += (sender, e) =>
            {
                Debug.WriteLine("Updated current location.");

                if (!e.Position.Location.IsUnknown)
                {
                    CurrentLocation = e.Position.Location;
                    geoCoordinateWatcher.Stop();
                }

                // now reverse geocode the coordinates
                var request = String.Format("http://maps.googleapis.com/maps/api/geocode/json?latlng={0},{1}&sensor=true", CurrentLocation.Latitude, CurrentLocation.Longitude);
                var webClient = new WebClient();                
                webClient.DownloadStringAsync(new Uri(request, UriKind.Absolute));
                webClient.DownloadStringCompleted += (wc_sender, wc_e) =>
                {
                    string city = null;
                    string state = null;
                    try
                    {
                        var json = JObject.Parse(wc_e.Result);
                        foreach (var component in json["results"][0]["address_components"].Children())
                        {
                            foreach (var type in component["types"].Children())
                            {
                                var value = type.Value<string>();
                                if (value == "locality")
                                {
                                    city = component["long_name"].Value<string>();
                                }
                                else if (value == "administrative_area_level_1")
                                {
                                    state = component["short_name"].Value<string>();
                                }
                                else if (value == "country" && state == null)
                                {
                                    state = component["short_name"].Value<string>(); // we'll use country if no state is available
                                }
                            }
                        }
                        CurrentLocationName = String.Format("{0}, {1}", city, state).ToUpper();                        
                    }
                    catch
                    {
                        CurrentLocationName = "UNKNOWN";
                    }
                    Debug.WriteLine("Updated current location name: " + CurrentLocationName);
                };
            };                    
        }
    }
}
