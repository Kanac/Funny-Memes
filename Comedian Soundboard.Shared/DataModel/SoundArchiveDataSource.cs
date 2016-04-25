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
    public sealed class SoundArchiveDataSource
    {
        private static readonly SoundArchiveDataSource _soundArchiveDataSource = new SoundArchiveDataSource();
        private HttpClient httpClient = new HttpClient();

        /// <summary>
        // Retrieves comedian files from Soundarchive and parses them into Categories. The existingCategories parameter
        // is optional for the case of comparing for duplicate titles 
        /// </summary>
        public async static Task<ICollection<Category>> GetSoundboardAudioFiles(ICollection<Category> existingCategories = null)
        {
            string mainHtml = await _soundArchiveDataSource.httpClient.GetStringAsync("http://www.thesoundarchive.com/");

            HtmlDocument mainDoc = new HtmlAgilityPack.HtmlDocument();
            mainDoc.LoadHtml(mainHtml);
            HtmlNode htmlList = mainDoc.DocumentNode.Descendants("ul").Where(x => x.Attributes.Contains("class") && x.Attributes["class"].Value.Contains("cbp-rfgrid")).FirstOrDefault();
            IEnumerable<HtmlNode> htmlATags = htmlList.Descendants("a");

            ICollection<Category> comedians = new List<Category>();
            foreach (HtmlNode htmlATag in htmlATags)
            {
                string title = htmlATag.Descendants("h3").FirstOrDefault().InnerHtml;
                if (title.Contains("<br>")){
                    title = title.Substring(0, title.IndexOf("<br>"));
                }
                title = SoundDataSource.HumanizeAudioTitle(title, maxWords:6);
                string uniqueId = title;
                string imageUrl = "http://www.thesoundarchive.com/" + htmlATag.Descendants("img").FirstOrDefault().Attributes["src"].Value;
                if (existingCategories != null && existingCategories.Where(x => x.UniqueId == title).Count() > 0) {
                    uniqueId += "1";  // Ensure no duplicate uniqueIds
                }
                Category comedian = new Category(uniqueId, title, "", imageUrl, "");

                string link = "http://www.thesoundarchive.com/" + htmlATag.Attributes["href"].Value;
                string pageHtml = await _soundArchiveDataSource.httpClient.GetStringAsync(link);
                HtmlDocument pageDoc = new HtmlDocument();
                pageDoc.LoadHtml(pageHtml);
                HtmlNode audioList = pageDoc.DocumentNode.Descendants("div").Where(x => x.Attributes.Contains("id") && x.Attributes["id"].Value.Contains("page-content")).FirstOrDefault();
                IEnumerable<HtmlNode> htmlLiTags = audioList.Descendants("li");

                foreach (HtmlNode htmlLiTag in htmlLiTags)
                {
                    string soundTitle = htmlLiTag.InnerText;
                    soundTitle = soundTitle.Replace("&quot;", "");
                    if (soundTitle.Contains("- |")){
                        soundTitle = soundTitle.Substring(0, soundTitle.IndexOf("- |"));
                    }
                    soundTitle = SoundDataSource.HumanizeAudioTitle(soundTitle, maxWords: 3);

                    if (htmlLiTag.Descendants("a").Where(x => x.InnerText.Contains("MP3")).Count() > 0)
                    {
                        string rawSoundUrl = htmlLiTag.Descendants("a").Where(x => x.InnerText.Contains("MP3")).FirstOrDefault().Attributes["href"].Value;
                        rawSoundUrl = rawSoundUrl.Replace("play-wav-files.asp?sound=", "");
                        string soundUrl = "http://www.thesoundarchive.com/" + rawSoundUrl;
                        comedian.SoundItems.Add(new SoundItem("","",soundTitle, soundUrl,"","", true));
                    }
                }
                comedians.Add(comedian);
            }
            return comedians;
        }
    }
}
