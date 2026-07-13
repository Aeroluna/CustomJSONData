#if PRE_V1_37_1
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using _LightColorBaseData = BeatmapSaveDataVersion3.BeatmapSaveData.LightColorBaseData;
using _LightColorEventBox = BeatmapSaveDataVersion3.BeatmapSaveData.LightColorEventBox;
using _LightColorEventBoxConverter = BeatmapDataLoader.LightColorEventBoxConvertor;

namespace CustomJSONData.HarmonyPatches
{
    // Pre-1.37.1 version of the GLS converter patches. ConvertersCustomify.cs handles 1.37.1+ because
    // the LightColorEventBoxConverter type moved to BeatmapDataLoaderVersion3.BeatmapDataLoader.
    [HarmonyPatch]
    internal static class GlsConverterPatches
    {
        private static readonly ConditionalWeakTable<LightColorBeatmapEventDataBox, List<CustomData?>> _boxCustomData
            = new();

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(_LightColorEventBoxConverter),
            "Convert")]
        private static void LightColorEventBoxConverterPostfix(
            _LightColorEventBox saveData,
            BeatmapEventDataBox __result)
        {
            if (__result is not LightColorBeatmapEventDataBox box)
            {
                return;
            }

            List<CustomData?> perEventData = new();
            bool anyCustom = false;
            foreach (_LightColorBaseData item in saveData.lightColorBaseDataList ?? new List<_LightColorBaseData>())
            {
                if (item is ICustomData cd && cd.customData.Count > 0)
                {
                    perEventData.Add(cd.customData);
                    anyCustom = true;
                }
                else
                {
                    perEventData.Add(null);
                }
            }

            if (anyCustom)
            {
                _boxCustomData.Add(box, perEventData);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(LightColorBeatmapEventDataBox), "Unpack")]
        private static void LightColorBeatmapEventDataBoxUnpackPostfix(
            LightColorBeatmapEventDataBox __instance,
            List<BeatmapEventData> output)
        {
            if (!_boxCustomData.TryGetValue(__instance, out List<CustomData?> perEventData))
            {
                return;
            }

            int boxCount = perEventData!.Count;
            int outputStart = output.Count - boxCount;
            if (outputStart < 0)
            {
                return;
            }

            for (int i = 0; i < boxCount; i++)
            {
                CustomData? customData = perEventData[i];
                if (customData == null)
                {
                    continue;
                }

                int outIdx = outputStart + i;
                if (output[outIdx] is not LightColorBeatmapEventData ev || ev is CustomLightColorBeatmapEventData)
                {
                    continue;
                }

                output[outIdx] = new CustomLightColorBeatmapEventData(
                    ev.time,
                    ev.groupId,
                    ev.elementId,
                    ev.transitionType,
                    ev.colorType,
                    ev.brightness,
                    ev.strobeBeatFrequency,
#if !V1_29_1
                    ev.strobeBrightness,
                    ev.strobeFade,
#endif
                    customData);
            }
        }
    }
}
#endif
