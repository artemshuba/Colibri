using System;
using System.Globalization;
using Windows.System;

namespace Colibri.Helpers
{
    public static class MapHelper
    {
        private const string _mapUrl = "https://maps.googleapis.com/maps/api/staticmap?center={0}&zoom={1}&size=200x200&maptype=roadmap&language={2}&key={3}&markers=color:red|{0}";
        private const string _apiKey = "AIzaSyA77rDM6itomvbf2nQ2L9SR1riXAMEvXnQ";
        private const int _zoom = 14;

        public static string GetMapPreviewForCoords(double latitude, double longitude)
        {
            return string.Format(_mapUrl, latitude.ToString(CultureInfo.InvariantCulture) + "," + longitude.ToString(CultureInfo.InvariantCulture), _zoom, CultureInfo.CurrentUICulture.TwoLetterISOLanguageName, _apiKey);
        }

        public static async void OpenMap(double latitude, double longitude, string title = "")
        {
            var result = await Launcher.LaunchUriAsync(new Uri($"bingmaps:?collection=point.{latitude.ToString(CultureInfo.InvariantCulture)}_{longitude.ToString(CultureInfo.InvariantCulture)}_{title}&lvl=14"));
            if (!result)
            {
                await Launcher.LaunchUriAsync(new Uri($"https://www.google.com/maps/?q={latitude.ToString(CultureInfo.InvariantCulture)},{longitude.ToString(CultureInfo.InvariantCulture)}"));
            }
        }
    }
}