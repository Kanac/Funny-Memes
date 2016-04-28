using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.Notifications;

namespace Tasks
{
    public sealed class ToastBackground : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            if (ToastNotificationManager.CreateToastNotifier().GetScheduledToastNotifications().Where(x => x.Id == "Background").Count() > 0)
            {
                return;
            }

            ToastTemplateType toastTemplate = ToastTemplateType.ToastImageAndText02;
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);

            XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode("Comedy"));
            toastTextElements[1].AppendChild(toastXml.CreateTextNode("New comedy audio has arrived! Come check it out!"));

            IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
            XmlElement audio = toastXml.CreateElement("audio");

            // Get a random comedian audio and play it for the toast
            //StorageFolder appFolder = await Package.Current.InstalledLocation.GetFolderAsync(@"Assets\Comedians\");
            //Random random = new Random();
            //IReadOnlyList<StorageFolder> comedianFolders = await appFolder.GetFoldersAsync();
            //StorageFolder randComedian = comedianFolders.ElementAt(random.Next(0, comedianFolders.Count()));
            //StorageFolder randComedianSounds = await randComedian.GetFolderAsync("Sounds");
            //IReadOnlyList<StorageFile> randComedianSoundFiles = await randComedianSounds.GetFilesAsync();
            //StorageFile randComedianSound = randComedianSoundFiles.ElementAt(random.Next(0, randComedianSoundFiles.Count()));

            //audio.SetAttribute("src", "ms-appx:///Assets/" + randComedian.DisplayType + "/Sounds/" + randComedianSound.DisplayName + randComedianSound.FileType);
            //audio.SetAttribute("src", "ms-appx:///Assets/Adam Sandler/Crazy Guy.mp3");
            //toastNode.AppendChild(audio);

            ToastNotification toast = new ToastNotification(toastXml);
            DateTime dueTime = DateTime.Now.AddHours(50);
            ScheduledToastNotification scheduledToast = new ScheduledToastNotification(toastXml, dueTime);
            scheduledToast.Id = "Background";
            ToastNotificationManager.CreateToastNotifier().AddToSchedule(scheduledToast);
        }
    }
}