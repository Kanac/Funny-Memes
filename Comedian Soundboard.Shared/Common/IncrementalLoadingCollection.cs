using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Comedian_Soundboard.Common
{
    public interface IIncrementalSource<T>
    {
        Task<IEnumerable<T>> GetPagedItems();
    }

    public class IncrementalLoadingCollection<T, I> : ObservableCollection<I>,
         ISupportIncrementalLoading
         where T : IIncrementalSource<I>, new()
    {
        private T source;
        private bool hasMoreItems;

        public IncrementalLoadingCollection()
        {
            this.source = new T();
            this.hasMoreItems = true;
        }

        public IncrementalLoadingCollection(IEnumerable<I> collection)
        {
            this.source = new T();
            this.hasMoreItems = true;
            foreach (var item in collection) {
                this.Add(item);
            }
        }

        public bool HasMoreItems
        {
            get { return hasMoreItems; }
        }

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            var dispatcher = Window.Current.Dispatcher;

            return Task.Run<LoadMoreItemsResult>(
                async () =>
                {
                    uint resultCount = 0;
                    var result = await source.GetPagedItems();

                    if (result == null || result.Count() == 0)
                    {
                        hasMoreItems = false;
                    }
                    else
                    {
                        resultCount = (uint)result.Count();

                        await dispatcher.RunAsync(
                            CoreDispatcherPriority.Normal,
                            () =>
                            {
                                foreach (I item in result)
                                    this.Add(item);
                            });
                    }

                    return new LoadMoreItemsResult() { Count = resultCount };

                }).AsAsyncOperation<LoadMoreItemsResult>();
        }
    }
}
