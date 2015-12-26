using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The data model defined by this file serves as a representative example of a strongly-typed
// model.  The property names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs. If using this model, you might improve app 
// responsiveness by initiating the data loading task in the code behind for App.xaml when the app 
// is first launched.

namespace Comedian_Soundboard.Data
{
    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class SoundItem
    {
        public SoundItem(String uniqueId, String title, String subtitle, String soundPath, String description, String content)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Subtitle = subtitle;
            this.Description = description;
            this.SoundPath = soundPath;
            this.Content = content;
        }

        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public string Description { get; private set; }
        public string SoundPath { get; private set; }
        public string Content { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class Category
    {
        public Category(String uniqueId, String title, String subtitle, String imagePath, String description)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Subtitle = subtitle;
            this.Description = description;
            this.ImagePath = imagePath;
            this.SoundItems = new ObservableCollection<SoundItem>();
        }

        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public string Subtitle { get; set; }
        public string Description { get; private set; }
        public string ImagePath { get; set; }
        public ObservableCollection<SoundItem> SoundItems { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with content read from a static json file.
    /// 
    /// SampleDataSource initializes with data read from a static json file included in the 
    /// project.  This provides sample data at both design-time and run-time.
    /// </summary>
    public sealed class SoundDataSource
    {
        private static SoundDataSource _soundDataSource = new SoundDataSource();

        private ObservableCollection<Category> _categories = new ObservableCollection<Category>();
        public ObservableCollection<Category> Categories
        {
            get { return this._categories; }
        }

        public static async Task<IEnumerable<Category>> GetCategoryAsync()
        {
            await _soundDataSource.GetSoundDataAutomatedAsync();

            return _soundDataSource.Categories;
        }

        public static async Task<Category> GetCategoryAsync(string uniqueId)
        {
            await _soundDataSource.GetSoundDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _soundDataSource.Categories.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static async Task<SoundItem> GetSoundAsync(string uniqueId)
        {
            await _soundDataSource.GetSoundDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _soundDataSource.Categories.SelectMany(group => group.SoundItems).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        private async Task GetSoundDataAutomatedAsync() {
            if (this._categories.Count != 0)
                return;

            StorageFolder appFolder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync(@"Assets\Comedians\");
            IReadOnlyList<StorageFolder> comedianFolders = await appFolder.GetFoldersAsync();
            foreach (StorageFolder currComedianFolder in comedianFolders) {
                IReadOnlyList<StorageFile> currComedianFiles = await currComedianFolder.GetFilesAsync();  // Should only contain comedian image
                string currComedianImagePath = "Assets/Comedians/" + currComedianFolder.DisplayName + "/" + currComedianFiles.FirstOrDefault().Name;

                Category currComedian = new Category("", currComedianFolder.DisplayName, "", currComedianImagePath, "");

                StorageFolder currComedianSoundFolder = await currComedianFolder.GetFolderAsync("Sounds");
                IReadOnlyList<StorageFile> comedianSounds = await currComedianSoundFolder.GetFilesAsync();

                foreach (StorageFile currComedianSound in comedianSounds) {
                    string currComedianSoundPath = "Assets/Comedians/" + currComedianFolder.DisplayName + "/" + currComedianSound.Name;
                    currComedian.SoundItems.Add(
                        new SoundItem("", "", currComedianSound.DisplayName, "", currComedianSoundPath, ""));
                }

                this.Categories.Add(currComedian);
            }
        }

        private async Task GetSoundDataAsync()
        {
            if (this._categories.Count != 0)
                return;

            Uri dataUri = new Uri("ms-appx:///DataModel/Soundboard.json");

            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
            string jsonText = await FileIO.ReadTextAsync(file);
            JsonObject jsonObject = JsonObject.Parse(jsonText);
            JsonArray jsonArray = jsonObject["Groups"].GetArray();

            foreach (JsonValue groupValue in jsonArray)
            {
                JsonObject groupObject = groupValue.GetObject();
                Category group = new Category(groupObject["UniqueId"].GetString(),
                                                            groupObject["Title"].GetString(),
                                                            groupObject["Subtitle"].GetString(),
                                                            groupObject["ImagePath"].GetString(),
                                                            groupObject["Description"].GetString());

                foreach (JsonValue itemValue in groupObject["SoundItems"].GetArray())
                {
                    JsonObject itemObject = itemValue.GetObject();
                    group.SoundItems.Add(new SoundItem(itemObject["UniqueId"].GetString(),
                                                       itemObject["Title"].GetString(),
                                                       itemObject["Subtitle"].GetString(),
                                                       itemObject["SoundPath"].GetString(),
                                                       itemObject["Description"].GetString(),
                                                       itemObject["Content"].GetString()));
                }
                this.Categories.Add(group);
            }
        }
    }
}