using System;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;

namespace CustomJSONData.HarmonyPatches
{
    [HarmonyPatch(typeof(BeatmapData))]
    internal static class BeatmapDataCustomify
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(BeatmapData.AddBeatmapObjectData))]
        private static void PreAddBeatmapObjectData(BeatmapObjectData beatmapObjectData, BeatmapData __instance)
        {
            ((CustomBeatmapData)__instance).PreAddBeatmapObjectData(beatmapObjectData);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(BeatmapData.AddBeatmapObjectDataInOrder))]
        private static void PreAddBeatmapObjectDataInOrder(BeatmapObjectData beatmapObjectData, BeatmapData __instance)
        {
            ((CustomBeatmapData)__instance).PreAddBeatmapObjectData(beatmapObjectData);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(BeatmapData.InsertBeatmapEventData))]
        private static void PreInsertBeatmapEventData(BeatmapEventData beatmapEventData, BeatmapData __instance)
        {
            ((CustomBeatmapData)__instance).PreInsertBeatmapEventData(beatmapEventData);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(BeatmapData.InsertBeatmapEventDataInOrder))]
        private static void PreInsertBeatmapEventDataInOrder(BeatmapEventData beatmapEventData, BeatmapData __instance)
        {
            ((CustomBeatmapData)__instance).PreInsertBeatmapEventData(beatmapEventData);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(BeatmapData.GetCopy))]
        public static bool GetCopy(BeatmapData __instance, ref BeatmapData __result)
        {
            CustomBeatmapData original = (CustomBeatmapData)__instance;
            CustomBeatmapData beatmapData = new(
                original._numberOfLines,
                original.version2_6_0AndEarlier,
                original.customData.Copy(),
                original.beatmapCustomData.Copy(),
                original.levelCustomData.Copy());
            foreach (BeatmapDataItem beatmapDataItem in original.allBeatmapDataItems)
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
        public static bool GetFilteredCopy(Func<BeatmapDataItem, BeatmapDataItem> processDataItem, BeatmapData __instance, ref BeatmapData __result)
        {
            CustomBeatmapData original = (CustomBeatmapData)__instance;
            original._isCreatingFilteredCopy = true;
            CustomBeatmapData beatmapData = new(
                original._numberOfLines,
                original.version2_6_0AndEarlier,
                original.customData.Copy(),
                original.beatmapCustomData.Copy(),
                original.levelCustomData.Copy());
            foreach (BeatmapDataItem beatmapDataItem in original.allBeatmapDataItems)
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

            original._isCreatingFilteredCopy = false;
            __result = beatmapData;
            return false;
        }
    }
}
