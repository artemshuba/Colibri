using System;
using System.Collections.Generic;
using System.IO;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using Colibri.Controls;
using Colibri.Helpers;
using Colibri.Services;
using Jupiter.Utils.Extensions;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Colibri.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PhotosPreviewView : Page
    {
        private string _currentPhoto;
        private List<string> _photos;

        public PhotosPreviewView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var p = e.Parameter as Dictionary<string, object>;
            if (p != null)
            {
                SaveButton.IsEnabled = true;

                if (p.ContainsKey("photos"))
                {
                    _photos = (List<string>)p["photos"];
                    FlipView.ItemsSource = _photos;
                }

                if (p.ContainsKey("currentPhoto"))
                    _currentPhoto = (string)p["currentPhoto"];

                if (p.ContainsKey("currentPhotoSource"))
                {
                    var imageSource = (ImageSource)p["currentPhotoSource"];
                    FlipView.ItemsSource = new List<ImageSource>() { imageSource };
                    SaveButton.IsEnabled = false;
                }
            }

            base.OnNavigatedTo(e);
        }

        private void UIElement_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            PopupControl.CloseCurrent();
        }

        private void FlipView_OnLoaded(object sender, RoutedEventArgs e)
        {
            //if (!_photos.IsNullOrEmpty())
            //    FlipView.SelectedIndex = _photos.IndexOf(_currentPhoto); //TODO not working sometimes
        }

        private async void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            //TODO
            string currentPhoto = null;
            if (_photos.IsNullOrEmpty() || _photos.Count == 1)
                currentPhoto = _currentPhoto;
            else
                currentPhoto = _photos[FlipView.SelectedIndex]; //(string)FlipView.SelectedItem;

            var picker = new FileSavePicker();
            picker.FileTypeChoices.Add("Image", new List<string>() { Path.GetExtension(currentPhoto) });
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            var file = await picker.PickSaveFileAsync();
            if (file != null)
            {

                try
                {
                    var httpClient = new HttpClient();
                    var imageStream = await httpClient.GetInputStreamAsync(new Uri(currentPhoto));
                    var fileStream = await file.OpenStreamForWriteAsync();
                    await imageStream.AsStreamForRead().CopyToAsync(fileStream);

                    await fileStream.FlushAsync();
                    fileStream.Dispose();
                    imageStream.Dispose();

                    await new MessageDialog(Localizator.String("Errors/SaveImageDialogCommonSuccess"), Localizator.String("Errors/SaveImageDialogTitleDone")).ShowAsync();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Unable to save image");

                    await new MessageDialog(Localizator.String("Errors/SaveImageDialogCommonError"), Localizator.String("Errors/SaveImageDialogTitleError")).ShowAsync();
                }
            }
        }
    }
}
