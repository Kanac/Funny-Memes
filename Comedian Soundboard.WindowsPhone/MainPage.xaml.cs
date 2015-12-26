using Comedian_Soundboard.Common;
using Comedian_Soundboard.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Email;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Phone.Devices.Notification;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Comedian_Soundboard
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        private Random random = new Random();

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public MainPage()
        {
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            var groups = await SoundDataSource.GetCategoryAsync();
            this.DefaultViewModel["Groups"] = groups;
            reviewApp();
        }

        private void Group_Click(object sender, ItemClickEventArgs e)
        {
            VibrationDevice testVibrationDevice = VibrationDevice.GetDefault();
            testVibrationDevice.Vibrate(TimeSpan.FromMilliseconds(101));
            Frame.Navigate(typeof(AudioPage), ((Category)e.ClickedItem).UniqueId);
        }

        private async void Comment_Click(object sender, RoutedEventArgs e)
        {
            EmailRecipient sendTo = new EmailRecipient() { Address = "testgglol@outlook.com" };
            EmailMessage mail = new EmailMessage();

            mail.Subject = "Comedian Suggestion for Comedy Soundboard";
            mail.To.Add(sendTo);
            await EmailManager.ShowComposeNewEmailAsync(mail);
        }

        private async void Rate_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=" + CurrentApp.AppId));
        }

        private async void Lucky_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<Category> comedians = await SoundDataSource.GetCategoryAsync();
            Category randComedian = comedians.ElementAt(random.Next(0, comedians.Count()));
            SoundItem randSound = randComedian.SoundItems.ElementAt(random.Next(0, randComedian.SoundItems.Count()));
            Audio.Source = new Uri("ms-appx:///" + randSound.SoundPath, UriKind.RelativeOrAbsolute);

        }
        private void Audio_MediaOpened(object sender, RoutedEventArgs e)
        {
            Audio.Play();
        }
        private void Pointer_Pressed(object sender, PointerRoutedEventArgs e)
        {
            Border border = (sender as Image).FindName("ImageBorder") as Border;
            border.BorderThickness = new Thickness(8);
        }

        private void Pointer_Released(object sender, PointerRoutedEventArgs e)
        {
            Border border = (sender as Image).FindName("ImageBorder") as Border;
            border.BorderThickness = new Thickness(3);
        }
        private void Border_Loaded(object sender, RoutedEventArgs e)
        {
            Color color = Color.FromArgb(255, Convert.ToByte(random.Next(0, 256)), Convert.ToByte(random.Next(0, 256)), Convert.ToByte(random.Next(0, 256)));
            (sender as Border).BorderBrush = new SolidColorBrush(color);
        }
        private async void reviewApp()
        {
            if (!localSettings.Values.ContainsKey("Views"))
                localSettings.Values.Add(new KeyValuePair<string, object>("Views", 3));
            else
                localSettings.Values["Views"] = 1 + Convert.ToInt32(localSettings.Values["Views"]);

            int viewCount = Convert.ToInt32(localSettings.Values["Views"]);


            // Only ask for review up to several times, once every 4 times this page is visited, and do not ask anymore once reviewed
            if (viewCount % 4 == 0 && viewCount <= 50 && Convert.ToInt32(localSettings.Values["Rate"]) != 1)
            {
                var reviewBox = new MessageDialog("Keep updates coming by rating this app 5 stars to support us!");
                reviewBox.Commands.Add(new UICommand { Label = "Yes! :)", Id = 0 });
                reviewBox.Commands.Add(new UICommand { Label = "Maybe later", Id = 1 });

                var reviewResult = await reviewBox.ShowAsync();
                if ((int)reviewResult.Id == 0)
                {
                    try {
                        await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=" + CurrentApp.AppId));
                    }
                    catch (Exception e) {
                        reviewBox = new MessageDialog("An error has occured! " + e.Message);
                        await reviewBox.ShowAsync();
                    }

                    localSettings.Values["Rate"] = 1;
                }
            }
        }
    }
}
