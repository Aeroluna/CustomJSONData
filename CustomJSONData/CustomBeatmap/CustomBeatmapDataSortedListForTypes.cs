using System;
using System.Collections.Generic;
using BSIPA_Utilities;
using HarmonyLib;

namespace CustomJSONData.CustomBeatmap
{
    [HarmonyPatch(typeof(BeatmapDataSortedListForTypeAndIds<BeatmapDataItem>))]
    public class CustomBeatmapDataSortedListForTypeAndIds : BeatmapDataSortedListForTypeAndIds<BeatmapDataItem>
    {
        private static readonly FieldAccessor<BeatmapDataSortedListForTypeAndIds<BeatmapDataItem>, Dictionary<ValueTuple<Type, int>, ISortedList<BeatmapDataItem>>>.Accessor _itemsAccessor =
            FieldAccessor<BeatmapDataSortedListForTypeAndIds<BeatmapDataItem>, Dictionary<ValueTuple<Type, int>, ISortedList<BeatmapDataItem>>>.GetAccessor(nameof(_items));

        public CustomBeatmapDataSortedListForTypeAndIds(BeatmapDataSortedListForTypeAndIds<BeatmapDataItem> original)
        {
            BeatmapDataSortedListForTypeAndIds<BeatmapDataItem> @this = this;
            _itemsAccessor(ref @this) = original._items;
            _sortedListsDataProcessors.Add(typeof(CustomEventData), null);
        }

        // GetType is a real stinky way of indexing stuff
        [HarmonyPrefix]
        [HarmonyPatch(nameof(BeatmapDataSortedListForTypeAndIds<BeatmapDataItem>.InsertItem))]
        public static bool InsertItem(BeatmapDataSortedListForTypeAndIds<BeatmapDataItem> __instance, BeatmapDataItem item, ref LinkedListNode<BeatmapDataItem> __result)
        {
            if (__instance is not CustomBeatmapDataSortedListForTypeAndIds customInstance)
            {
                return true;
            }

            __result = customInstance._itemToNodeMap[item] = customInstance.GetList(CustomBeatmapData.GetCustomType(item), item.subtypeGroupIdentifier).Insert(item);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(BeatmapDataSortedListForTypeAndIds<BeatmapDataItem>.RemoveItem))]
        public static bool RemoveItem(BeatmapDataSortedListForTypeAndIds<BeatmapDataItem> __instance, BeatmapDataItem item)
        {
            if (__instance is not CustomBeatmapDataSortedListForTypeAndIds customInstance)
            {
                return true;
            }

            ISortedList<BeatmapDataItem> list = customInstance.GetList(CustomBeatmapData.GetCustomType(item), item.subtypeGroupIdentifier);
            if (customInstance._itemToNodeMap.TryGetValue(item, out LinkedListNode<BeatmapDataItem> node))
            {
                list.Remove(node);
            }

            return false;
        }
    }
}
