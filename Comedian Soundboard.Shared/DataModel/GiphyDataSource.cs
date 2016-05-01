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
    public sealed class GiphyDataSource : IIncrementalSource<AnimationItem>
    {
        public static readonly GiphyDataSource Current = new GiphyDataSource();

        private HttpClient _HttpClient = new HttpClient();
        private readonly int MAX_PAGES = 999;
        private readonly int PAGES_PER_CRAWL = 1;
        private int _Page = 1;  // current page of site
        private int _PagesCrawled = 0;
        private int _Count = 0; // total gifs crawled
        private bool _HasCrawledAll = false;
        private Random _Random = new Random();

        public async Task<IEnumerable<AnimationItem>> GetPagedItems()
        {
            ICollection<AnimationItem> animations = new List<AnimationItem>();
            string mainHtml = await Current._HttpClient.GetStringAsync("http://giphy.com/page" + Current._Page + "/");

            int currPage = 0;
            while (currPage < PAGES_PER_CRAWL && Current._PagesCrawled < MAX_PAGES && !Current._HasCrawledAll)
            {
                GetAnimationsFromHtml(mainHtml, animations);

                ++currPage;
                ++Current._Page;
                ++Current._PagesCrawled;
            }

            return animations;
        }

        public async Task<IEnumerable<AnimationItem>> GetSampleItems()
        {
            // Assuming this method is called first, this will set the initial page to crawl from for GetPagedItems()
            Current._Page = Current._Random.Next(100);

            ICollection<AnimationItem> animations = new List<AnimationItem>();
            string mainHtml = await Current._HttpClient.GetStringAsync("http://giphy.com/page" + Current._Page + "/");

            GetAnimationsFromHtml(mainHtml, animations);
            AnimationItem more = new AnimationItem("See More", "Assets/Comedy.png");

            List<AnimationItem> filteredGifs = animations.Take(3).ToList();
            filteredGifs.Add(more);
            return filteredGifs;
        }

        private void GetAnimationsFromHtml(string html, ICollection<AnimationItem> animations)
        {
            HtmlDocument mainDoc = new HtmlDocument();
            mainDoc.LoadHtml(html);

            IEnumerable<HtmlNode> gifDivs = mainDoc.DocumentNode.Descendants("a").Where(x => x.Attributes.Contains("class") && x.Attributes["class"].Value == "gif-link");
            IEnumerable<HtmlNode> tagDivs = mainDoc.DocumentNode.Descendants("span").Where(x => x.Attributes.Contains("class") && x.Attributes["class"].Value == "caption tags");

            if (gifDivs.Count() == 0 || tagDivs.Count() == 0)
            {
                Current._HasCrawledAll = true;
                return;
            }

            for (int i = 0; i < gifDivs.Count(); ++i)
            {
                HtmlNode currGifDiv = gifDivs.ElementAt(i);
                HtmlNode currTagDiv = tagDivs.ElementAt(i);

                HtmlNode gifNode = currGifDiv.Descendants("img").FirstOrDefault();
                string url = gifNode.Attributes["animated"].Value;

                StringBuilder titleSb = new StringBuilder();
                IEnumerable<HtmlNode> tagNodes = currGifDiv.Descendants("a");
                foreach(HtmlNode tag in tagNodes)
                {
                    string val = tag.InnerText;
                    titleSb.Append("#" + val + " ");
                }

                AnimationItem gifItem = new AnimationItem(titleSb.ToString(), url);
                animations.Add(gifItem);

                ++Current._Count;
            }
        }
    }
}
