using Comedian_Soundboard.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace Comedian_Soundboard.DataModel
{
    public class ImageItem
    {
        public ImageItem(string title, string url)
        {
            this.Title = title;
            this.ImagePath = url;
        }

        public string Title { get; private set; }

        public string ImagePath { get; private set; }
    }

    public sealed class ImageDataSource
    {
        private static readonly ImageDataSource _ImageDataSource = new ImageDataSource();

        private ObservableCollection<ImageItem> _Images = new IncrementalLoadingCollection<MemesDataSource, ImageItem>();
        public ObservableCollection<ImageItem> Images
        {
            get { return this._Images; }
        }

        public static ObservableCollection<ImageItem> GetImages()
        {
            return _ImageDataSource.Images;
        }

        public static async Task<ObservableCollection<ImageItem>> GetSampleImages()
        {
            IEnumerable<ImageItem> sample = await MemesDataSource.Current.GetSampleItems();
            return new ObservableCollection<ImageItem>(sample);
        }
    }
}
