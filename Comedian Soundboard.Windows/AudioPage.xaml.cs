using Comedian_Soundboard.Common;
using Comedian_Soundboard.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Universal Hub Application project template is documented at http://go.microsoft.com/fwlink/?LinkID=391955

namespace Comedian_Soundboard
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class AudioPage : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");
        private DispatcherTimer timer = new DispatcherTimer();
        private ProgressBar currentProgressBar;
        private Brush initColour;
        private Random random = new Random();

        public AudioPage()
        {
            this.InitializeComponent();

            // Hub is only supported in Portrait orientation
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;

            this.NavigationCacheMode = NavigationCacheMode.Required;

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
            // Set data context of current comedian category
            string categoryId = (string)e.NavigationParameter;
            Category category = await SoundDataSource.GetCategoryAsync(categoryId);
            DefaultViewModel["Category"] = category;
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
        private void Back_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void Sound_Click(object sender, TappedRoutedEventArgs e)
        {
            SoundItem soundItem = (SoundItem)(((FrameworkElement)e.OriginalSource).DataContext);
            Audio.Source = new Uri("ms-appx:///" +  soundItem.SoundPath, UriKind.RelativeOrAbsolute);

            if (currentProgressBar != null)
            {
                timer.Stop();
                timer.Tick -= Timer_Tick;
                currentProgressBar.Value = 0;
            }

            currentProgressBar = ((FrameworkElement)sender).FindName("ProgressBar") as ProgressBar;
        }

        private void Audio_MediaOpened(object sender, RoutedEventArgs e)
        {
            // Update progress bar of audio at 100 hz 
            // Calling when media is opened otherwise NaturalDuration will not return a correct value
            Audio.Play();
            currentProgressBar.Value = 0;
            double stepSize = Audio.NaturalDuration.TimeSpan.TotalMilliseconds / (100.0);
            timer.Interval = new TimeSpan(0, 0, 0, 0, (int)stepSize);
            timer.Start();
            timer.Tick += Timer_Tick;


        }

        private void Timer_Tick(object sender, object e)
        {
            if (Audio.Position.TotalMilliseconds >= Audio.NaturalDuration.TimeSpan.TotalMilliseconds)
            {
                timer.Stop();
                timer.Tick -= Timer_Tick;
                currentProgressBar.Value = 0;
            }
            else {
                currentProgressBar.Value = Audio.Position.TotalMilliseconds / Audio.NaturalDuration.TimeSpan.TotalMilliseconds * 100;
            }
        }

        private async void Save_Clicked(object sender, RoutedEventArgs e)
        {
            if (currentProgressBar == null)
                return;

            FileSavePicker fileSavePicker = new FileSavePicker();
            SoundItem selectedSound = currentProgressBar.DataContext as SoundItem;

            Uri audioPath = new Uri("ms-appx:///" + selectedSound.SoundPath);
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(audioPath);

            fileSavePicker.SuggestedSaveFile = file;
            fileSavePicker.SuggestedFileName = selectedSound.Subtitle;
            fileSavePicker.FileTypeChoices.Add("MP3", new List<string>() { ".mp3" });
            StorageFile pickedFile = await fileSavePicker.PickSaveFileAsync();

            if (pickedFile != null)
            {
                CachedFileManager.DeferUpdates(pickedFile);
                await file.CopyAndReplaceAsync(pickedFile);
                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(pickedFile);
            }
        }

        private void Pointer_Pressed(object sender, PointerRoutedEventArgs e)
        {
            ProgressBar progressBar = sender as ProgressBar;
            initColour = progressBar.Background;
            Color accent = ((Color)Application.Current.Resources["SystemColorControlAccentColor"]);
            progressBar.Background = new SolidColorBrush(Color.FromArgb(50, accent.R, accent.G, accent.B));
        }

        private void Pointer_Released(object sender, PointerRoutedEventArgs e)
        {
            ProgressBar progressBar = (ProgressBar)sender;
            if (initColour != null)
                progressBar.Background = initColour;
        }
        private void PointerImage_Pressed(object sender, PointerRoutedEventArgs e)
        {
            (sender as Image).Opacity = 0.5;
        }

        private void PointerImage_Released(object sender, PointerRoutedEventArgs e)
        {
            (sender as Image).Opacity = 1;
        }

        private void ProgressBar_Loaded(object sender, RoutedEventArgs e)
        {
            ProgressBar progressBar = (ProgressBar)sender;
            Color accent = ((Color)Application.Current.Resources["SystemColorControlAccentColor"]);
            progressBar.Background = new SolidColorBrush(accent);
            progressBar.Foreground = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));
        }
    }
}