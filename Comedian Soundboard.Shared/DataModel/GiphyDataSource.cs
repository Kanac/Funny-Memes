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
        private readonly int _GifsPerGrab = 7;
        private int _TotalCalls = 0;
        private int _Page = 1;  // current page of site
        private int _PagesCrawled = 0;
        private int _Count = 0; // total gifs crawled
        private bool _HasCrawledAll = false;
        private Random _Random = new Random();
        private ICollection<AnimationItem> _Animations = new List<AnimationItem>();

        // giphy redirects page url requests to page 1, change approach later
        public async Task<IEnumerable<AnimationItem>> GetPagedItems()
        {
            string mainHtml = await Current._HttpClient.GetStringAsync("http://giphy.com/page/" + Current._Page + "/");

            int currPage = 0;
            while (currPage < PAGES_PER_CRAWL && Current._PagesCrawled < MAX_PAGES && !Current._HasCrawledAll)
            {
                GetAnimationsFromHtml(mainHtml, _Animations);

                ++currPage;
                ++Current._Page;
                ++Current._PagesCrawled;

                // since page url requests redirect to page 1, 1 html fetch is enough
                Current._HasCrawledAll = true;
            }

            Current._TotalCalls++;
            int startIndex = (Current._TotalCalls - 1) * Current._GifsPerGrab;
            if ((_Animations.Count - 1) < (startIndex + Current._GifsPerGrab))
            {
                return new List<AnimationItem>();
            }

            return _Animations.Skip(startIndex).Take(Current._GifsPerGrab);
        }

        public async Task<IEnumerable<AnimationItem>> GetSampleItems()
        {
            // Assuming this method is called first, this will set the initial page to crawl from for GetPagedItems()
            Current._Page = Current._Random.Next(100);

            ICollection<AnimationItem> animations = new List<AnimationItem>();
            string mainHtml = await Current._HttpClient.GetStringAsync("http://giphy.com/page/" + Current._Page + "/");

            GetAnimationsFromHtml(mainHtml, animations, 3);
            AnimationItem more = new AnimationItem("See More", "Assets/Comedy.png");

            animations.Add(more);
            return animations;
        }
        // maxImages = 0 means it will get as many as possible from the html

        private void GetAnimationsFromHtml(string html, ICollection<AnimationItem> animations, int maxGifs = 0)
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

            for (int i = 0; i < gifDivs.Count() && (i < maxGifs || maxGifs == 0); ++i)
            {
                HtmlNode currGifDiv = gifDivs.ElementAt(i);
                HtmlNode currTagDiv = tagDivs.ElementAt(i);

                HtmlNode gifNode = currGifDiv.Descendants("img").FirstOrDefault();
                string url = gifNode.Attributes["data-animated"].Value;

                StringBuilder titleSb = new StringBuilder();
                IEnumerable<HtmlNode> tagNodes = currTagDiv.Descendants("a");
                foreach(HtmlNode tag in tagNodes)
                {
                    string val = tag.InnerText.Trim();
                    titleSb.Append(val + " ");
                }

                AnimationItem gifItem = new AnimationItem(titleSb.ToString(), url);
                animations.Add(gifItem);

                ++Current._Count;
            }
        }
    }
}
