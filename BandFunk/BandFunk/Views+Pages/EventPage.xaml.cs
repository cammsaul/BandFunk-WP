using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Device.Location;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Controls.Maps;

namespace BandFunk
{
    public partial class EventPage : PhoneApplicationPage
    {
        private string EventId { get; set; }
        private Event _Event;
        private Event Event
        {
            get
            {
                return _Event;
            }
            set
            {
                _Event = value;

                LayoutRoot.DataContext = Event;
                if (Event.Venue.Latitude != null && Event.Venue.Longitude != null)
                {
                    var location = new GeoCoordinate(Event.Venue.Latitude.Value, Event.Venue.Longitude.Value);
                    Map.Center = location;

                    // set the location pushpin
                    Map.Children.Clear();
                    var pushpin = new Pushpin();
                    pushpin.Location = location;
                    Map.Children.Add(pushpin);
                }
                else
                {
                    Map.Visibility = Visibility.Collapsed;
                }
            }
        }

        public EventPage()
        {
            InitializeComponent();

            FetcherLastFM.EventFetched += anEvent =>
            {
                if (anEvent.Id == EventId)
                {
                    Event = anEvent;
                    ProgressBar.Visibility = Visibility.Collapsed;
                    UpdateAppBarButtons();
                }
            };

            if (Utility.IsWindowsPhone8OrHigher)
            {
                // hide the website button (move to menu), show a calendar button instead
                foreach (ApplicationBarIconButton button in ApplicationBar.Buttons)
                {
                    if (button.Text == "website") {
                        ApplicationBar.Buttons.Remove(button);
                        break;
                    }
                }

                var menuItem = new ApplicationBarMenuItem("view on Last.FM");
                menuItem.Click += WebsiteButton_Click;
                ApplicationBar.MenuItems.Add(menuItem);

                var calendarButton = new ApplicationBarIconButton(new Uri("/Images/appbar.feature.calendar.png", UriKind.Relative));
                calendarButton.Text = "save";
                calendarButton.Click += calendarButton_Click;
                ApplicationBar.Buttons.Add(calendarButton);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            EventId = NavigationContext.QueryString["eventID"];

            if (EventStore.Events.ContainsKey(EventId))
            {
                Event = EventStore.Events[EventId];
                ProgressBar.Visibility = Visibility.Collapsed;
                UpdateAppBarButtons();
            }
            else
            {
                FetcherLastFM.FetchEvent(EventId);
            }
        }

        private void UpdateAppBarButtons()
        {
            foreach (ApplicationBarIconButton button in ApplicationBar.Buttons)
            {
                if (button.Text == "pin to start")
                {
                    button.IsEnabled = EventTileManager.EnableAddLiveTile(Event);
                }
            }
        }

        private void OtherArtistsTextBlock_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var artistName = (sender as TextBlock).Text;
            NavigationService.Navigate(new Uri(String.Format("/Views+Pages/ArtistPivotPage.xaml?artistName={0}", Uri.EscapeDataString(artistName)), UriKind.Relative));
        }

        private void ShareEventButton_Click_1(object sender, EventArgs e)
        {
            ShareManager.Share(Event);
        }

        private void ShareEventViaTextMessageBarMenuItem_Click_1(object sender, EventArgs e)
        {
            ShareManager.ShareTextMessage(Event);
        }

        private void PinToStartButton_Click(object sender, EventArgs e)
        {
            EventTileManager.PinToStart(Event);
            UpdateAppBarButtons();
        }

        private void PhoneButton_Click(object sender, EventArgs e)
        {
            var phoneCallTask = new PhoneCallTask();
            phoneCallTask.DisplayName = Event.Venue.Title;
            phoneCallTask.PhoneNumber = Event.Venue.PhoneNumber;
            phoneCallTask.Show();
        }

        private void WebsiteButton_Click(object sender, EventArgs e)
        {
            var webBrowserTask = new WebBrowserTask();
            webBrowserTask.Uri = Event.LastFMUri;
            webBrowserTask.Show();
        }

        private void ShareEventViaEmailBarMenuItem_Click_1(object sender, EventArgs e)
        {
            ShareManager.ShareTextMessage(Event);
        }

        private void ContextMenu_Map_Click_1(object sender, RoutedEventArgs e)
        {
            var mapsTask = new BingMapsTask();
            mapsTask.SearchTerm = Event.Venue.Title;
            mapsTask.Center = new GeoCoordinate(Event.Venue.Latitude.Value, Event.Venue.Longitude.Value);
            mapsTask.ZoomLevel = 13;
            mapsTask.Show();
        }

        void calendarButton_Click(object sender, EventArgs e)
        {
            Type saveAppointemntTaskType = Type.GetType("Microsoft.Phone.Tasks.SaveAppointmentTask, Microsoft.Phone");
            Type reminderType = Type.GetType("Microsoft.Phone.Tasks.Reminder, Microsoft.Phone");
            var saveAppointmentTask = saveAppointemntTaskType.GetConstructor(new Type[] { }).Invoke(null);

            SetProperty(saveAppointmentTask, "StartTime", Event.Date);
            SetProperty(saveAppointmentTask, "Subject", Event.Title);
            SetProperty(saveAppointmentTask, "Location", String.Format("{0}, {1}, {2}", Event.Venue.Title, Event.Venue.StreetAddress, Event.Venue.City));
            SetProperty(saveAppointmentTask, "Details", String.Format("Phone for {0}: {1}", Event.Venue.Title, Event.Venue.PhoneNumber)); 
            SetProperty(saveAppointmentTask, "Reminder", Enum.Parse(enumType:reminderType, value:"OneWeek", ignoreCase:true));

            saveAppointemntTaskType.GetMethod("Show").Invoke(saveAppointmentTask, null);
        }

        private static void SetProperty(object instance, string name, object value)
        {
            var setMethod = instance.GetType().GetProperty(name).GetSetMethod();
            setMethod.Invoke(instance, new object[] { value });
        }
    }
}