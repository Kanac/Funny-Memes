using Comedian_Soundboard.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace Comedian_Soundboard.DataModel
{
    public class AnimationItem
    {
        public AnimationItem(string title, string url)
        {
            this.Title = title;
            this.GifPath = url;
        }

        public string Title { get; private set; }

        public string GifPath { get; private set; }
    }

    public sealed class AnimationDataSource
    {
        private static readonly AnimationDataSource _AnimationDataSource = new AnimationDataSource();

        private ObservableCollection<AnimationItem> _Animations = new IncrementalLoadingCollection<GiphyDataSource, AnimationItem>();
        public ObservableCollection<AnimationItem> Animations
        {
            get { return this._Animations; }
        }

        public static ObservableCollection<AnimationItem> GetAnimations()
        {
            return _AnimationDataSource.Animations;
        }

        public static async Task<ObservableCollection<AnimationItem>> GetSampleAnimations()
        {
            IEnumerable<AnimationItem> sample = await GiphyDataSource.Current.GetSampleItems();
            return new ObservableCollection<AnimationItem>(sample);
        }
    }
}
