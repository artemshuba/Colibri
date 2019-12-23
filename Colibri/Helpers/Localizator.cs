using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Resources.Core;

namespace Colibri.Helpers
{
    public static class Localizator
    {
        public static string String(string key)
        {
            if (!key.Contains("/"))
            {
                key = "Resources/" + key;
            }

            var resource = ResourceManager.Current.MainResourceMap.GetValue(key, ResourceContext.GetForCurrentView());
            return resource?.ValueAsString;
        }
    }
}