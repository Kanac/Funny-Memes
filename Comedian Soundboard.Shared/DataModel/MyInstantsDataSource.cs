using Comedian_Soundboard.Common;
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
    public sealed class MyInstantsDataSource : IIncrementalSource<Category>
    {
        public static readonly MyInstantsDataSource Current = new MyInstantsDataSource();

        private HttpClient _HttpClient = new HttpClient();
        private readonly int MAX_CATEGORY_AUDIO_FILES = 15;
        private readonly int MAX_PAGES = 99;
        private readonly int PAGES_PER_CRAWL = 1;
        private int _Page = 1;  // current page of site
        private int _Count = 0; // total audio mp3s crawled
        private bool _HasCrawledAll = false;

        public async Task<IEnumerable<Category>> GetPagedItems()
        {
            ICollection<Category> categories = new List<Category>();
            string mainHtml = mainHtml = await _HttpClient.GetStringAsync("http://www.myinstants.com" + "?page=" + _Page);
            
            int currPage = 0;
            while (currPage < PAGES_PER_CRAWL && _Page < MAX_PAGES && !_HasCrawledAll)
            {
                HtmlDocument mainDoc = new HtmlDocument();
                mainDoc.LoadHtml(mainHtml);
                IEnumerable<HtmlNode> audioDivs = mainDoc.DocumentNode.Descendants("div").Where(x => x.Attributes.Contains("class") && x.Attributes["class"].Value == "instant");

                string categoryTitle = "Random " + (_Count / MAX_CATEGORY_AUDIO_FILES + 1).ToString();
                Category currCategory = new Category(categoryTitle, categoryTitle, "", "Assets/QuestionMark.png", "");
                categories.Add(currCategory);

                int currCount = 0;
                foreach (HtmlNode ad in audioDivs)
                {
                    if (currCount % MAX_CATEGORY_AUDIO_FILES == 0 && currCount != 0)
                    {
                        categoryTitle = "Random " + (_Count / MAX_CATEGORY_AUDIO_FILES + 1).ToString();
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

                    ++_Count;
                    ++currCount;
                }

                if (mainDoc.DocumentNode.Descendants("a").Where(x => x.Attributes.Contains("id") && x.Attributes["id"].Value == "moar").Count() > 0)
                {
                    mainHtml = await _HttpClient.GetStringAsync("http://www.myinstants.com" + "?page=" + ++_Page);
                }
                else
                {
                    _HasCrawledAll = true;
                    break;
                }

                ++currPage;
            } 
            return categories;
        }
    }
}
