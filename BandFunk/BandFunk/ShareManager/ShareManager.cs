using Microsoft.Phone.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BandFunk
{
    public static class ShareManager
    {
        public static void Share(Artist artist)
        {
            ShareLinkTask shareLinkTask = new ShareLinkTask();
            shareLinkTask.Title = artist.Name;
            shareLinkTask.LinkUri = artist.LastFMUri;
            shareLinkTask.Message = String.Format("Check out {0} on #BandFunk!", artist.Name);
            shareLinkTask.Show();
        }

        public static void ShareTextMessage(Artist artist)
        {
            var smsComposeTask = new SmsComposeTask();
            smsComposeTask.Body = String.Format("Check out {0}! {1}", artist.Name, artist.LastFMUri);
            smsComposeTask.Show();
        }

        public static void ShareEmail(Artist artist)
        {
            var emailComposeTask = new EmailComposeTask();
            emailComposeTask.Subject = String.Format("Check out {0}!", artist.Name);
            emailComposeTask.Body = String.Format("Check out {0} on Last.FM: {1}\n\nCheck out Band Funk: {2}", artist.Name, artist.LastFMUri, "http://getluckybird.com/bandfunk");
            emailComposeTask.Show();
        }

        public static void Share(Event anEvent)
        {
            var shareLinkTask = new ShareLinkTask();
            shareLinkTask.Title = anEvent.Title;
            shareLinkTask.LinkUri = anEvent.LastFMUri;
            shareLinkTask.Message = String.Format("Check out {0} at {1} on {2}! #BandFunk!", anEvent.Title, anEvent.Venue.Title, anEvent.Date.ToShortDateString());
            shareLinkTask.Show();
        }

        public static void ShareTextMessage(Event anEvent)
        {
            var smsComposeTask = new SmsComposeTask();
            smsComposeTask.Body = string.Format("Check out {0} at {1} on {2}! {3}", anEvent.Title, anEvent.Venue.Title, anEvent.Date.ToShortDateString(), anEvent.LastFMUri);
            smsComposeTask.Show();
        }

        public static void ShareEmail(Event anEvent)
        {
            var emailComposeTask = new EmailComposeTask();
            emailComposeTask.Subject = String.Format("Check out {0} at {1} on {2}!", anEvent.Title, anEvent.Venue.Title, anEvent.Date.ToShortDateString());
            emailComposeTask.Body = String.Format("Check out {0} at {1} on {2} on Last.FM: {3}\n\nCheck out Band Funk: {4}", anEvent.Title, anEvent.Venue.Title, anEvent.Date.ToShortDateString(), anEvent.LastFMUri, "http://getluckybird.com/bandfunk");
            emailComposeTask.Show();
        }

        public static void Share(Genre genre)
        {
            var shareLinkTask = new ShareLinkTask();
            shareLinkTask.Title = genre.Name;
            shareLinkTask.LinkUri = genre.LastFMUri;
            shareLinkTask.Message = String.Format("Check out {0} on #BandFunk!", genre.Name);
            shareLinkTask.Show();
        }

        public static void ShareTextMessage(Genre genre)
        {
            var smsComposeTask = new SmsComposeTask();
            smsComposeTask.Body = String.Format("Check out {0}! {1} #BandFunk!", genre.Name, genre.LastFMUri.OriginalString);
            smsComposeTask.Show();
        }

        public static void ShareEmail(Genre genre)
        {
            var emailComposeTask = new EmailComposeTask();
            emailComposeTask.Subject = String.Format("Check out {0}!", genre.Name);
            emailComposeTask.Body = String.Format("Check out {0} on Last.FM: {1}\n\nCheck out Band Funk: {2}", genre.Name, genre.LastFMUri.OriginalString, "http://getluckybird.com/bandfunk");
            emailComposeTask.Show();
        }
    }
}
