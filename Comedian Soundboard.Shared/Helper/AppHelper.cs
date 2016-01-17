using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Background;
using Windows.System;
using Windows.UI.Popups;

namespace Comedian_Soundboard.Helper
{
    public sealed class AppHelper
    {
        private static readonly AppHelper _appHelper = new AppHelper();

        private Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

        private async void CheckAppVersion()
        {
            String appVersion = String.Format("{0}.{1}.{2}.{3}",
                    Package.Current.Id.Version.Build,
                    Package.Current.Id.Version.Major,
                    Package.Current.Id.Version.Minor,
                    Package.Current.Id.Version.Revision);

            String lastVersion = Windows.Storage.ApplicationData.Current.LocalSettings.Values["AppVersion"] as String;

            if (lastVersion == null || lastVersion != appVersion)
            {
                // Our app has been updated
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["AppVersion"] = appVersion;

                // Call RemoveAccess
                BackgroundExecutionManager.RemoveAccess();
            }

            BackgroundAccessStatus status = await BackgroundExecutionManager.RequestAccessAsync();
        }
        public static async void ReviewApp()
        {
            if (!_appHelper.localSettings.Values.ContainsKey("Views"))
                _appHelper.localSettings.Values.Add(new KeyValuePair<string, object>("Views", 3));
            else
                _appHelper.localSettings.Values["Views"] = 1 + Convert.ToInt32(_appHelper.localSettings.Values["Views"]);

            int viewCount = Convert.ToInt32(_appHelper.localSettings.Values["Views"]);

            // Only ask for review up to several times, once every 4 times this page is visited, and do not ask anymore once reviewed
            if (viewCount % 4 == 0 && viewCount <= 50 && Convert.ToInt32(_appHelper.localSettings.Values["Rate"]) != 1)
            {
                var reviewBox = new MessageDialog("Keep updates coming by rating this app 5 stars to support us!");
                reviewBox.Commands.Add(new UICommand { Label = "Yes! :)", Id = 0 });
                reviewBox.Commands.Add(new UICommand { Label = "Maybe later", Id = 1 });

                var reviewResult = await reviewBox.ShowAsync();
                if ((int)reviewResult.Id == 0)
                {
                    try
                    {
                        await Launcher.LaunchUriAsync(new Uri(string.Format("ms-windows-store:REVIEW?PFN={0}", Windows.ApplicationModel.Package.Current.Id.FamilyName)));
                    }
                    catch (Exception e)
                    {
                        var errorBox = new MessageDialog("An error has occured! " + e.Message);
                        await errorBox.ShowAsync();
                    }
                    _appHelper.localSettings.Values["Rate"] = 1;
                }
            }
        }
        public static async Task SetupBackgroundToast()
        {
            _appHelper.CheckAppVersion();
            var toastTaskName = "ToastBackgroundTask";
            var taskRegistered = false;

            foreach (var task in Windows.ApplicationModel.Background.BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == toastTaskName)
                {
                    taskRegistered = true;
                    break;
                }
            }
            if (!taskRegistered)
            {
                await Windows.ApplicationModel.Background.BackgroundExecutionManager.RequestAccessAsync();
                var builder = new BackgroundTaskBuilder();
                builder.Name = toastTaskName;
                builder.TaskEntryPoint = "Tasks.ToastBackground";
                var hourlyTrigger = new TimeTrigger(30, false);
                builder.SetTrigger(hourlyTrigger);

                BackgroundTaskRegistration task = builder.Register();
            }
        }
    }
}
