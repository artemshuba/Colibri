using Windows.UI.Xaml;

namespace Colibri.Model
{
    public class SettingsAppTheme
    {
        public string Title { get; set; }

        public ElementTheme Value { get; set; }

        public SettingsAppTheme(string title, ElementTheme value)
        {
            Title = title;
            Value = value;
        }
    }
}
