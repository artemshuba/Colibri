using Windows.Foundation;
using Windows.UI.Xaml.Data;

namespace Colibri.Helpers
{
    public interface ISupportTopIncrementalLoading
    {
        bool HasMoreItemsOnTop { get; }

        IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsOnTopAsync(uint count);
    }
}
