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
        private readonly int MAX_PAGES = 99;
        private readonly int PAGES_PER_CRAWL = 1;
        private int _Page = 1;  // current page of site
        private int _Count = 0; // total images crawled
        private bool _HasCrawledAll = false;

        public async Task<IEnumerable<ImageItem>> GetPagedItems()
        {
            ICollection<ImageItem> images = new List<ImageItem>();
            string mainHtml = await _HttpClient.GetStringAsync("http://www.quickmeme.com/page/" + _Page + "/");

            int currPage = 0;
            while (currPage < PAGES_PER_CRAWL && _Page < MAX_PAGES && !_HasCrawledAll)
            {
                GetImagesFromHtml(mainHtml, images);

                ++currPage;
                ++_Page;
            }

            return images;
        }

        public async Task<IEnumerable<ImageItem>> GetSampleItems()
        {
            ICollection<ImageItem> images = new List<ImageItem>();
            string mainHtml = await _HttpClient.GetStringAsync("http://www.quickmeme.com/page/1/");

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
                _HasCrawledAll = true;
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

                ++_Count;
            }
        }
    }
}
