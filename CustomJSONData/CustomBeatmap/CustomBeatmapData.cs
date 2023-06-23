using System;
using System.Collections.Generic;
using HarmonyLib;
using IPA.Utilities;
using JetBrains.Annotations;

namespace CustomJSONData.CustomBeatmap
{
    [HarmonyPatch(typeof(BeatmapData))]
    public sealed class CustomBeatmapData : BeatmapData
    {
        private static readonly FieldAccessor<BeatmapData, BeatmapDataSortedListForTypeAndIds<BeatmapDataItem>>.Accessor _beatmapDataItemsPerTypeAccessor =
            FieldAccessor<BeatmapData, BeatmapDataSortedListForTypeAndIds<BeatmapDataItem>>.GetAccessor(nameof(_beatmapDataItemsPerTypeAndId));

        private readonly List<BeatmapObjectData> _beatmapObjectDatas = new();
        private readonly List<BeatmapEventData> _beatmapEventDatas = new();
        private readonly List<CustomEventData> _customEventDatas = new();

        public CustomBeatmapData(
            int numberOfLines,
            bool version2_6_0AndEarlier,
            CustomData customData,
            CustomData beatmapCustomData,
            CustomData levelCustomData)
            : base(numberOfLines)
        {
            BeatmapData @this = this;
            _beatmapDataItemsPerTypeAccessor(ref @this) =
                new CustomBeatmapDataSortedListForTypeAndIds<BeatmapDataItem>(_beatmapDataItemsPerTypeAccessor(ref @this));
            this.version2_6_0AndEarlier = version2_6_0AndEarlier;
            this.customData = customData;
            this.beatmapCustomData = beatmapCustomData;
            this.levelCustomData = levelCustomData;
        }

        public bool version2_6_0AndEarlier { get; }

        public CustomData customData { get; }

        public CustomData beatmapCustomData { get; }

        public CustomData levelCustomData { get; }

        [PublicAPI]
        public IReadOnlyList<BeatmapObjectData> beatmapObjectDatas => _beatmapObjectDatas;

        [PublicAPI]
        public IReadOnlyList<BeatmapEventData> beatmapEventDatas => _beatmapEventDatas;

        [PublicAPI]
        public IReadOnlyList<CustomEventData> customEventDatas => _customEventDatas;

        public static Type GetCustomType(object item)
        {
            Type type = item.GetType();
            if (item is not CustomEventData && item is ICustomData)
            {
                type = type.BaseType
                       ?? throw new InvalidOperationException($"[{item.GetType().FullName}] does not have a base type.");
            }

            return type;
        }

        internal void PreAddBeatmapObjectData(BeatmapObjectData beatmapObjectData)
        {
            _beatmapObjectDatas.Add(beatmapObjectData);
        }

        internal void PreInsertBeatmapEventData(BeatmapEventData beatmapEventData)
        {
            _beatmapEventDatas.Add(beatmapEventData);
        }

        public void InsertCustomEventData(CustomEventData customEventData)
        {
            _customEventDatas.Add(customEventData);
            LinkedListNode<BeatmapDataItem> node = _beatmapDataItemsPerTypeAndId.InsertItem(customEventData);
            if (updateAllBeatmapDataOnInsert)
            {
                InsertToAllBeatmapData(customEventData, node);
            }
        }

        // what in gods name is the point of this
        public void InsertCustomEventDataInOrder(CustomEventData customEventData)
        {
            InsertCustomEventData(customEventData);
            InsertToAllBeatmapData(customEventData);
        }
    }
}
