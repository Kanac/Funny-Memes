using Comedian_Soundboard.DataModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Data.Json;
using Windows.Foundation.Metadata;
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
        public string Subtitle { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }
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
        private static readonly SoundDataSource _soundDataSource = new SoundDataSource();
        private int onlineOffset = 0;

        private ObservableCollection<Category> _categories = new ObservableCollection<Category>();
        public ObservableCollection<Category> Categories
        {
            get { return this._categories; }
        }

        public static async Task<IEnumerable<Category>> GetOnlineCategoriesAsync() {
            await _soundDataSource.GetOnlineSoundDataAsync();

            return _soundDataSource.Categories.Skip(_soundDataSource.onlineOffset);
        }

        public static async Task<IEnumerable<Category>> GetCategoriesAsync()
        {
            await _soundDataSource.GetSoundDataAutomatedAsync();

            return _soundDataSource.Categories;
        }

        public static async Task<Category> GetCategoryAsync(string uniqueId)
        {
            await _soundDataSource.GetSoundDataAutomatedAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _soundDataSource.Categories.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) {
                 await _soundDataSource.GetSoundItemAutomatedAsync(matches.First());
                 return matches.First();
            }
            return null;
        }

        public static async Task<SoundItem> GetSoundAsync(string uniqueId)
        {
            await _soundDataSource.GetSoundDataAutomatedAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _soundDataSource.Categories.SelectMany(group => group.SoundItems).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        // Helper method that changes the input string to match given constraints such that only the first 3 words are taken and 
        // if those 3 words have already been used before (recorded by dictionary input wordCount) it will list a number suffix to it as well
        private string humanizeAudioTitle(string title, Dictionary<string, int> wordCount) {
            var words = title.Split(' ');
            int count = 0;
            StringBuilder audioName = new StringBuilder();
            foreach (var word in words)
            {
                if (count >= 3) break;
                if (word != "")
                {
                    audioName.Append(word[0].ToString().ToUpper());
                    if (word.Length > 1)
                        audioName.Append(word.Substring(1));

                    audioName.Append(" ");
                    count++;
                }
            }

            audioName.Remove(audioName.Length - 1, 1);
            string audioNameDisplay = audioName.ToString();
            if (wordCount.ContainsKey(audioName.ToString()))
            {
                wordCount[audioName.ToString()]++;
                audioNameDisplay += " " + wordCount[audioName.ToString()];
            }
            else
                wordCount.Add(audioName.ToString(), 1);

            return audioNameDisplay;
        }

        private async Task GetOnlineSoundDataAsync() {
            if (this.onlineOffset != 0)
                return;

            this.onlineOffset = _soundDataSource.Categories.Count();  // Get the offset to the online files in collection before adding
            ICollection<Category> onlineComedians = await SoundArchiveDataSource.GetSoundboardAudioFiles();
            foreach (Category comedian in onlineComedians) {
                this.Categories.Add(comedian);
            }

        }

        // Add an online category for purpose of a button in the main page list view
        private Category AddOnlineCategory() {
            return new Category("Search Online", "Search Online", "", "Assets/Chrome.png", "");
        }

        // Automatically parses the assets into objects for each comedian provided that files are located in the given hierarchy
        // Assets/Comedians/(x)/ is the location of the (only) file for iamge
        // Assets/Comedians/(x)/Sounds/ is the location of all the mp3 files
        private async Task GetSoundDataAutomatedAsync() {
            if (this._categories.Count != 0)
                return;

            this.Categories.Add(AddOnlineCategory());

            StorageFolder appFolder = await Package.Current.InstalledLocation.GetFolderAsync(@"Assets\Comedians\");
            IReadOnlyList<StorageFolder> comedianFolders = await appFolder.GetFoldersAsync();
            foreach (StorageFolder currComedianFolder in comedianFolders) {
                IReadOnlyList<StorageFile> currComedianFiles = await currComedianFolder.GetFilesAsync();  // Should only contain comedian image
                string currComedianImagePath = "Assets/Comedians/" + currComedianFolder.DisplayName + "/" + currComedianFiles.FirstOrDefault().Name;

                Category currComedian = new Category(currComedianFolder.DisplayName, currComedianFolder.DisplayName, "", currComedianImagePath, "");
                this.Categories.Add(currComedian);

                await GetSoundItemAutomatedAsync(currComedian);
            }
        }

        // This method helps split the task of parsing the actual sound files for each comedian.
        // Modularizing this function so that not all comedian sounds are loaded on bootup 
        // Only load them when user clicks on a comedian to save app memory especially on 512 MB devices
        private async Task GetSoundItemAutomatedAsync(Category comedian) {
            if (comedian.SoundItems.Count != 0)
                return;

            StorageFolder currComedianSoundFolder = await Package.Current.InstalledLocation.GetFolderAsync(@"Assets\Comedians\" + comedian.Title + @"\Sounds\");
            IReadOnlyList<StorageFile> comedianSounds = await currComedianSoundFolder.GetFilesAsync();

            Dictionary<string, int> wordCount = new Dictionary<string, int>();

            foreach (StorageFile currComedianSound in comedianSounds)
            {
                string audioNameDisplay = humanizeAudioTitle(currComedianSound.DisplayName, wordCount);
                string currComedianSoundPath = "Assets/Comedians/" + comedian.Title + "/Sounds/" + currComedianSound.Name;

                comedian.SoundItems.Add(
                    new SoundItem("", "", audioNameDisplay, currComedianSoundPath, "", ""));
            }
        }

        // Originally used a json file to provide metadeta for audio files
        // Now automatically parsing the files through their folders and relying on data set in the file
        [Deprecated("Use GetSoundDataAutomatedAsync() instead", DeprecationType.Deprecate, 1) ]
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