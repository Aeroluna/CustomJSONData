using System;
using System.Collections.Generic;
using IPA.Utilities;

namespace CustomJSONData.CustomBeatmap
{
    public class CustomBeatmapDataSortedListForTypeAndIds<TBase> : BeatmapDataSortedListForTypeAndIds<TBase>
        where TBase : BeatmapDataItem
    {
        private static readonly FieldAccessor<BeatmapDataSortedListForTypeAndIds<TBase>, Dictionary<ValueTuple<Type, int>, ISortedList<TBase>>>.Accessor _itemsAccessor =
            FieldAccessor<BeatmapDataSortedListForTypeAndIds<TBase>, Dictionary<ValueTuple<Type, int>, ISortedList<TBase>>>.GetAccessor(nameof(_items));

        public CustomBeatmapDataSortedListForTypeAndIds(BeatmapDataSortedListForTypeAndIds<TBase> original)
        {
            BeatmapDataSortedListForTypeAndIds<TBase> @this = this;
            _itemsAccessor(ref @this) = _itemsAccessor(ref original);
            _sortedListsDataProcessors.Add(typeof(CustomEventData), null);
        }
    }
}
