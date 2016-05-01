using Comedian_Soundboard.Common;
using Comedian_Soundboard.Data;
using Comedian_Soundboard.DataModel;
using Comedian_Soundboard.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Email;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.System;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Comedian_Soundboard
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RootPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private ObservableCollection<Category> _SoundGroups;
        private ObservableCollection<ImageItem> _ImageGroups;
        private ObservableCollection<AnimationItem> _GifGroups;
        private Random _Random = new Random();

        public RootPage()
        {
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
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            _SoundGroups = new ObservableCollection<Category>(await SoundDataSource.GetSampleCategoriesAsync());
            this.DefaultViewModel["SoundGroups"] = _SoundGroups;
            this.DefaultViewModel["IsLoadingSound"] = false;

            _ImageGroups = new ObservableCollection<ImageItem>(await ImageDataSource.GetSampleImages());
            this.DefaultViewModel["ImageGroups"] = _ImageGroups;
            this.DefaultViewModel["IsLoadingImage"] = false;

            _GifGroups = new ObservableCollection<AnimationItem>(await AnimationDataSource.GetSampleAnimations());
            this.DefaultViewModel["GifGroups"] = _GifGroups;
            this.DefaultViewModel["IsLoadingGif"] = false;

            this.DefaultViewModel["LoadingVisibility"] = Visibility.Collapsed;

            AppHelper.ReviewApp();
            if (!App.FirstLoad)
            {
                await AppHelper.SetupBackgroundToast();
                AppHelper.setupReuseToast(50);
                AppHelper.setupReuseToast(50*80);
                App.FirstLoad = false;
            }
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
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void Border_Loaded(object sender, RoutedEventArgs e)
        {
            Color color = Color.FromArgb(255, Convert.ToByte(_Random.Next(0, 256)), Convert.ToByte(_Random.Next(0, 256)), Convert.ToByte(_Random.Next(0, 256)));
            (sender as Ellipse).Stroke = new SolidColorBrush(color);
        }

        private void Pointer_Pressed(object sender, PointerRoutedEventArgs e)
        {
            FrameworkElement image = (sender as FrameworkElement);
            Ellipse border = image.FindName("ImageBorder") as Ellipse;
            border.Width = 235;
            border.Height = 235;
            border.StrokeThickness = 8;
        }

        private void Pointer_Released(object sender, PointerRoutedEventArgs e)
        {
            FrameworkElement image = (sender as FrameworkElement);
            Ellipse border = image.FindName("ImageBorder") as Ellipse;
            border.Width = 228;
            border.Height = 228;
            border.StrokeThickness = 4;
        }

        private void Group_Click(object sender, TappedRoutedEventArgs e)
        {
            var dataContext = (e.OriginalSource as FrameworkElement).DataContext;

            if (dataContext is Category)
            {
                string comedian = ((e.OriginalSource as FrameworkElement).DataContext as Category).UniqueId;
                Frame.Navigate(typeof(MainPage), comedian);
            }
            else if (dataContext is ImageItem)
            {
                // pass index of clicked sample item 
                ImageItem image = dataContext as ImageItem;
                int index = _ImageGroups.IndexOf(image);
                if (image.Title == "See More")
                {
                    index = 0;
                }
                Frame.Navigate(typeof(ImagePage), index);
            }
            else 
            {
                // pass index of clicked sample item 
                AnimationItem gif = dataContext as AnimationItem;
                int index = _GifGroups.IndexOf(gif);
                if (gif.Title == "See More")
                {
                    index = 0;
                }
                Frame.Navigate(typeof(GifPage), index);
            }
        }

        #region CommandBar Events
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
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=" + "450ee59b-0aff-40b4-b896-0382d05d96ee"));
        }

        #endregion CommandBar Events
    }
}
