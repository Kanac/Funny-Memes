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
    public sealed class MemesDataSource : IIncrementalSource<ImageItem>
    {
        public static readonly MemesDataSource Current = new MemesDataSource();

        private HttpClient _HttpClient = new HttpClient();
        private readonly int MAX_PAGES = 999;
        private readonly int PAGES_PER_CRAWL = 1;
        private int _Page = 1;  // current page of site
        private int _PagesCrawled = 0;
        private int _Count = 0; // total images crawled
        private bool _HasCrawledAll = false;
        private Random _Random = new Random();

        public async Task<IEnumerable<ImageItem>> GetPagedItems()
        {
            ICollection<ImageItem> images = new List<ImageItem>();
            string mainHtml = await Current._HttpClient.GetStringAsync("http://www.quickmeme.com/page/" + Current._Page + "/");

            int currPage = 0;
            while (currPage < PAGES_PER_CRAWL && Current._PagesCrawled < MAX_PAGES && !Current._HasCrawledAll)
            {
                GetImagesFromHtml(mainHtml, images);

                ++currPage;
                ++Current._Page;
                ++Current._PagesCrawled;
            }

            return images;
        }

        public async Task<IEnumerable<ImageItem>> GetSampleItems()
        {
            // Assuming this method is called first, this will set the initial page to crawl from for GetPagedItems()
            Current._Page = Current._Random.Next(1000);

            ICollection<ImageItem> images = new List<ImageItem>();
            string mainHtml = await Current._HttpClient.GetStringAsync("http://www.quickmeme.com/page/" + Current._Page + "/");

            GetImagesFromHtml(mainHtml, images);
            ImageItem more = new ImageItem("See More", "Assets/Comedy.png");

            List<ImageItem> filteredImages = images.Take(3).ToList();
            filteredImages.Add(more);
            return filteredImages;
        }

        private void GetImagesFromHtml(string html, ICollection<ImageItem> images)
        {
            HtmlDocument mainDoc = new HtmlDocument();
            mainDoc.LoadHtml(html);
            IEnumerable<HtmlNode> imageDivs = mainDoc.DocumentNode.Descendants("div").Where(x => x.Attributes.Contains("class") && x.Attributes["class"].Value == "post");
            int test = imageDivs.Count();
            if (imageDivs.Count() == 0)
            {
                Current._HasCrawledAll = true;
                return;
            }

            foreach (HtmlNode div in imageDivs)
            {
                HtmlNode titleNode = div.Descendants("h2").FirstOrDefault();
                HtmlNode imgNode = div.Descendants("img").FirstOrDefault();

                if (titleNode == null || imgNode == null)
                    continue;

                string title = titleNode.Descendants("a").FirstOrDefault().InnerText;
                string url = imgNode.Attributes["src"].Value;

                ImageItem imageItem = new ImageItem(title, url);
                images.Add(imageItem);

                ++Current._Count;
            }
        }
    }
}
