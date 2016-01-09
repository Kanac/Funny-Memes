using Comedian_Soundboard.Common;
using Comedian_Soundboard.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.System;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Comedian_Soundboard
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        private Random random = new Random();

        public MainPage()
        {
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            var groups = await SoundDataSource.GetCategoryAsync();
            this.DefaultViewModel["Groups"] = groups;
            LoadingPanel.Visibility = Visibility.Collapsed;
            reviewApp();
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            // TODO: Save the unique state of the page here.
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Audio.Source = null;
            this.navigationHelper.OnNavigatedFrom(e);
        }
        #endregion

        private void Group_Click(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(AudioPage), ((e.OriginalSource as FrameworkElement).DataContext as Category).UniqueId);
        }

        private async void Comment_Click(object sender, RoutedEventArgs e)
        {
            string to = "testgglol@outlook.com";
            string subject = "Comedian Suggestion";
            string body = "Hi, here's an idea for your cool app...";
            string link = string.Format("mailto:{0}?subject={1}&amp;body={2}", to, subject, Uri.EscapeDataString(body));
            await Launcher.LaunchUriAsync(new Uri(link));
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
        private void Border_Loaded(object sender, RoutedEventArgs e)
        {
            Color color = Color.FromArgb(255, Convert.ToByte(random.Next(0, 256)), Convert.ToByte(random.Next(0, 256)), Convert.ToByte(random.Next(0, 256)));
            (sender as Ellipse).Stroke = new SolidColorBrush(color);
        }

        private void Pointer_Pressed(object sender, PointerRoutedEventArgs e)
        {
            FrameworkElement image = (sender as FrameworkElement);
            Ellipse border = image.FindName("ImageBorder") as Ellipse;
            border.StrokeThickness = 8;
            border.Width = 265;
            border.Height = 265;
        }
        private void Pointer_Released(object sender, PointerRoutedEventArgs e)
        {
            FrameworkElement image = (sender as FrameworkElement);
            Ellipse border = image.FindName("ImageBorder") as Ellipse;
            border.StrokeThickness = 4;
            border.Width = 258;
            border.Height = 258;
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
