using Comedian_Soundboard.Data;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Comedian_Soundboard.DataModel
{
    public sealed class MyInstantsDataSource
    {
        private static readonly MyInstantsDataSource _myInstantsDataSource = new MyInstantsDataSource();
        private HttpClient httpClient = new HttpClient();
        private static readonly int MAX_CATEGORY_AUDIO_FILES = 20;

        public async static Task<ICollection<Category>> GetSoundboardAudioFiles()
        {
            string mainHtml = await _myInstantsDataSource.httpClient.GetStringAsync("http://www.myinstants.com");

            HtmlDocument mainDoc = new HtmlAgilityPack.HtmlDocument();
            mainDoc.LoadHtml(mainHtml);
            IEnumerable<HtmlNode> audioDivs = mainDoc.DocumentNode.Descendants("div").Where(x => x.Attributes.Contains("class") && x.Attributes["class"].Value == "instant");

            int count = 0;
            ICollection<Category> categories = new List<Category>();
            Category currCategory = new Category("Random 1", "Random 1", "", "Assets/QuestionMark.png", "");
            categories.Add(currCategory);
            foreach (HtmlNode ad in audioDivs) {
                if (count % MAX_CATEGORY_AUDIO_FILES == 0 && count != 0) {
                    string categoryTitle = "Random " + (count / MAX_CATEGORY_AUDIO_FILES + 1).ToString(); 
                    currCategory = new Category(categoryTitle, categoryTitle, "", "Assets/QuestionMark.png", "");
                    categories.Add(currCategory);
                }

                HtmlNode smallButton = ad.Descendants("div").Where(x => x.Attributes.Contains("class") && x.Attributes["class"].Value == "small-button").FirstOrDefault();
                HtmlNode aTag = ad.Descendants("a").FirstOrDefault();

                string onclick = smallButton.Attributes["onclick"].Value;
                onclick = onclick.Replace("play('", "");
                onclick = onclick.Replace("')", "");
                string url = "http://www.myinstants.com" + onclick;

                string title = aTag.InnerText;
                title = SoundDataSource.HumanizeAudioTitle(title);

                currCategory.SoundItems.Add(new SoundItem("", "", title, url, "", "", true));
                ++count;
            }

            return categories;
        }

    }
}
