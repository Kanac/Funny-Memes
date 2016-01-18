using Comedian_Soundboard.Data;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Comedian_Soundboard.DataModel
{
    public sealed class SoundArchiveDataSource
    {
        private static readonly SoundArchiveDataSource _soundArchiveDataSource = new SoundArchiveDataSource();
        private HttpClient httpClient = new HttpClient();

        public async static void GetSoundboardAudioFiles()
        {
            string mainHtml = await _soundArchiveDataSource.httpClient.GetStringAsync("http://www.thesoundarchive.com/");

            HtmlDocument mainDoc = new HtmlAgilityPack.HtmlDocument();
            mainDoc.LoadHtml(mainHtml);
            HtmlNode htmlList = mainDoc.DocumentNode.Descendants("ul").Where(x => x.Attributes.Contains("class") && x.Attributes["class"].Value.Contains("cbp-rfgrid")).FirstOrDefault();
            IEnumerable<HtmlNode> htmlATags = htmlList.Descendants("a");

            foreach (HtmlNode htmlATag in htmlATags)
            {
                string title = htmlATag.InnerText;
                string imageUrl = "http://www.thesoundarchive.com/" + htmlATag.Descendants("img").FirstOrDefault().Attributes["src"].Value;
                Category comedian = new Category(title, title, "", imageUrl, "");

                string link = "http://www.thesoundarchive.com/" + htmlATag.Attributes["href"].Value;
                string pageHtml = await _soundArchiveDataSource.httpClient.GetStringAsync(link);
                HtmlDocument pageDoc = new HtmlAgilityPack.HtmlDocument();
                pageDoc.LoadHtml(pageHtml);
                HtmlNode audioList = pageDoc.DocumentNode.Descendants("div").Where(x => x.Attributes.Contains("id") && x.Attributes["id"].Value.Contains("page-content")).FirstOrDefault();
                IEnumerable<HtmlNode> htmlLiTags = audioList.Descendants("li");

                foreach (HtmlNode htmlLiTag in htmlLiTags)
                {
                    string soundTitle = htmlLiTag.InnerText;
                    if (htmlLiTag.Descendants("a").Where(x => x.InnerText.Contains("MP3")).Count() > 0)
                    {
                        string soundUrl = "http://www.thesoundarchive.com/" + htmlLiTag.Descendants("a").Where(x => x.InnerText.Contains("MP3")).FirstOrDefault().Attributes["href"].Value;
                        comedian.SoundItems.Add(new SoundItem("","",soundTitle, soundUrl,"",""));
                    }
                }
            }
        }
    }
}
