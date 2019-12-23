using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Colibri.Services;
using Jupiter.Utils.Helpers;

namespace Colibri.Helpers
{
    public static class TextBlockExtension
    {
        public static readonly DependencyProperty HyperlinkForegroundProperty = DependencyProperty.RegisterAttached(
            "HyperlinkForeground", typeof(Brush), typeof(TextBlockExtension), new PropertyMetadata(default(Brush)));

        public static void SetHyperlinkForeground(DependencyObject element, Brush value)
        {
            element.SetValue(HyperlinkForegroundProperty, value);
        }

        public static Brush GetHyperlinkForeground(DependencyObject element)
        {
            return (Brush)element.GetValue(HyperlinkForegroundProperty);
        }
        public static string GetFormattedText(DependencyObject obj) { return (string)obj.GetValue(FormattedTextProperty); }

        public static void SetFormattedText(DependencyObject obj, string value)
        {
            obj.SetValue(FormattedTextProperty, value);
        }

        public static readonly DependencyProperty FormattedTextProperty =
            DependencyProperty.Register("FormattedText", typeof(string), typeof(TextBlockExtension),
            new PropertyMetadata(string.Empty, (sender, e) =>
            {
                RichTextBlock tb = (RichTextBlock)sender;
                if (tb != null && e.NewValue != null)
                {
                    string source = e.NewValue.ToString();

                    tb.Blocks.Clear();

                    var p = new Paragraph();
                    tb.Blocks.Add(p);

                    var sb = new StringBuilder();

                    var enumerator = StringInfo.GetTextElementEnumerator(source);
                    while (enumerator.MoveNext())
                    {
                        string textElement = enumerator.GetTextElement();
                        string s;

                        if (Smiles.Base.TryGetValue(textElement, out s))
                        {
                            if (sb.Length > 0)
                            {
                                var currentText = sb.ToString();
                                AddRun(p, currentText);
                                sb.Clear();
                            }

                            AddSmile(p, s);
                        }
                        else
                            sb.Append(textElement);
                    }

                    if (sb.Length > 0)
                    {
                        var currentText = sb.ToString();
                        AddRun(p, currentText);
                        sb.Clear();
                    }
                }
            }));

        private static void AddRun(Paragraph p, string source)
        {
            // Working variables.
            int index = 0;
            string text;
            Run run;
            Hyperlink link;

            // Regex initialization.
            string pattern = @"(?:(?:http|https):\/\/)?([-a-zA-Z0-9а-яА-Я.]{2,256}\.[a-zа-я]{2,4})\b(?:\/[-a-zA-Z0-9а-яА-Я@:%_\+.~#?&//=]*)?";
            Regex regex = new Regex(pattern);

            var matches = regex.Matches(source);

            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    // Add text before match.
                    int matchIndex = match.Index;
                    text = source.Substring(index, matchIndex - index);
                    run = new Run();
                    run.Text = text;
                    p.Inlines.Add(run);

                    // Add match as hyperlink.
                    string hyper = match.Value;
                    link = new Hyperlink();
                    link.UnderlineStyle = UnderlineStyle.Single;
                    //var linkForeground = GetHyperlinkForeground(tb);
                    //if (linkForeground != null)
                    link.Foreground = run.Foreground; //linkForeground;
                    run = new Run();
                    run.Text = hyper;
                    link.Inlines.Add(run);

                    // Complete link if necessary.
                    if (!hyper.Contains("@") && !hyper.StartsWith("http"))
                    {
                        hyper = @"http://" + hyper;
                    }

                    if (hyper.Contains("@") && !hyper.StartsWith("mailto"))
                    {
                        hyper = @"mailto://" + hyper;
                    }

                    try
                    {
                        link.NavigateUri = new Uri(hyper);
                        p.Inlines.Add(link);

                        index = matchIndex + match.Length;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Unable to parse uri " + hyper);
                    }
                }

                // Add text after last match.
                text = source.Substring(index, source.Length - index);
                run = new Run();
                run.Text = text;
                p.Inlines.Add(run);
            }
            else
                p.Inlines.Add(new Run() { Text = source });
        }

        private static void AddSmile(Paragraph p, string smile)
        {
            var inlineContainer = new InlineUIContainer();

            var image = new Image();
            var scale = DeviceHelper.ResolutionScale == ResolutionScale.Scale100Percent ? 1 : 2;
            image.Source = new BitmapImage(new Uri($"ms-appx:///Resources/Images/Smiles/{scale}X/{smile}.png"));
            image.Stretch = Stretch.Fill;
            image.Height = 16;
            image.Width = 16;
            image.Margin = new Thickness(1, 0, 1, -2);
            inlineContainer.Child = image;
            p.Inlines.Add(inlineContainer);
        }
    }
}