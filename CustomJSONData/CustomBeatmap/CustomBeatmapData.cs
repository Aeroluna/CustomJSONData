using System;
using System.Collections.Generic;
using HarmonyLib;
using IPA.Utilities;
using JetBrains.Annotations;

namespace CustomJSONData.CustomBeatmap
{
    [HarmonyPatch(typeof(BeatmapData))]
    public sealed class CustomBeatmapData : BeatmapData, ICustomData, IVersionable
    {
        private static readonly FieldAccessor<BeatmapData, BeatmapDataSortedListForTypeAndIds<BeatmapDataItem>>.Accessor _beatmapDataItemsPerTypeAccessor =
            FieldAccessor<BeatmapData, BeatmapDataSortedListForTypeAndIds<BeatmapDataItem>>.GetAccessor(nameof(_beatmapDataItemsPerTypeAndId));

        private readonly List<BeatmapObjectData> _beatmapObjectDatas = new();
        private readonly List<BeatmapEventData> _beatmapEventDatas = new();
        private readonly List<CustomEventData> _customEventDatas = new();

        public CustomBeatmapData(
            int numberOfLines,
            CustomData beatmapCustomData,
            CustomData levelCustomData,
            CustomData customData,
            Version version)
            : base(numberOfLines)
        {
            BeatmapData @this = this;
            _beatmapDataItemsPerTypeAccessor(ref @this) =
                new CustomBeatmapDataSortedListForTypeAndIds(_beatmapDataItemsPerTypeAndId);
            this.beatmapCustomData = beatmapCustomData;
            this.levelCustomData = levelCustomData;
            this.customData = customData;
            this.version = version;
        }

        public CustomData customData { get; }

        public Version version { get; }

#if !PRE_V1_37_1
        public CustomData beatmapCustomData { get; internal set; }

        public CustomData levelCustomData { get; internal set; }
#else
        public CustomData beatmapCustomData { get; }

        public CustomData levelCustomData { get; }
#endif

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

        [HarmonyPrefix]
        [HarmonyPatch(nameof(BeatmapData.AddBeatmapObjectData))]
        public static void AddBeatmapObjectData(BeatmapData __instance, BeatmapObjectData beatmapObjectData)
        {
            if (__instance is CustomBeatmapData customBeatmapData)
            {
                customBeatmapData._beatmapObjectDatas.Add(beatmapObjectData);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(BeatmapData.InsertBeatmapEventData))]
        public static void InsertBeatmapEventData(BeatmapData __instance, BeatmapEventData beatmapEventData)
        {
            if (__instance is CustomBeatmapData customBeatmapData)
            {
                customBeatmapData._beatmapEventDatas.Add(beatmapEventData);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(BeatmapData.GetCopy))]
        public static bool GetCopy(BeatmapData __instance, ref BeatmapData __result)
        {
            if (__instance is not CustomBeatmapData customBeatmapData)
            {
                return true;
            }

            CustomBeatmapData beatmapData = new(
                customBeatmapData._numberOfLines,
                customBeatmapData.beatmapCustomData.Copy(),
                customBeatmapData.levelCustomData.Copy(),
                customBeatmapData.customData.Copy(),
                customBeatmapData.version);
            foreach (BeatmapDataItem beatmapDataItem in customBeatmapData.allBeatmapDataItems)
            {
                BeatmapDataItem copy = beatmapDataItem.GetCopy();
                switch (copy)
                {
                    case BeatmapEventData beatmapEventData:
                        beatmapData.InsertBeatmapEventDataInOrder(beatmapEventData);
                        break;
                    case BeatmapObjectData beatmapObjectData:
                        beatmapData.AddBeatmapObjectDataInOrder(beatmapObjectData);
                        break;
                    case CustomEventData customEventData:
                        beatmapData.InsertCustomEventDataInOrder(customEventData);
                        break;
                }
            }

            __result = beatmapData;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(BeatmapData.GetFilteredCopy))]
        public static bool GetFilteredCopy(BeatmapData __instance, Func<BeatmapDataItem, BeatmapDataItem> processDataItem, ref BeatmapData __result)
        {
            if (__instance is not CustomBeatmapData customBeatmapData)
            {
                return true;
            }

            customBeatmapData._isCreatingFilteredCopy = true;
            CustomBeatmapData beatmapData = new(
                customBeatmapData._numberOfLines,
                customBeatmapData.beatmapCustomData.Copy(),
                customBeatmapData.levelCustomData.Copy(),
                customBeatmapData.customData.Copy(),
                customBeatmapData.version);
            foreach (BeatmapDataItem beatmapDataItem in customBeatmapData.allBeatmapDataItems)
            {
                BeatmapDataItem copy = processDataItem(beatmapDataItem.GetCopy());
                if (copy != null)
                {
                    switch (copy)
                    {
                        case BeatmapEventData beatmapEventData:
                            beatmapData.InsertBeatmapEventDataInOrder(beatmapEventData);
                            break;
                        case BeatmapObjectData beatmapObjectData:
                            beatmapData.AddBeatmapObjectDataInOrder(beatmapObjectData);
                            break;
                        case CustomEventData customEventData:
                            beatmapData.InsertCustomEventDataInOrder(customEventData);
                            break;
                    }
                }
            }

            customBeatmapData._isCreatingFilteredCopy = false;
            __result = beatmapData;
            return false;
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
