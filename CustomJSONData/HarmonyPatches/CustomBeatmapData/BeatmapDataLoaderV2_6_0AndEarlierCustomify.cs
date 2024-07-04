#if LATEST
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BeatmapSaveDataVersion2_6_0AndEarlier;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;

namespace CustomJSONData.HarmonyPatches
{
    [HarmonyPatch(typeof(BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader))]
    internal static class BeatmapDataLoaderV2_6_0AndEarlierCustomify
    {
        private static readonly MethodInfo _deserialize = AccessTools.Method(typeof(Version2_6_0AndEarlierCustomBeatmapSaveData), nameof(Version2_6_0AndEarlierCustomBeatmapSaveData.Deserialize));

        private static readonly ConstructorInfo _beatmapDataCtor = AccessTools.FirstConstructor(typeof(BeatmapData), _ => true);
        private static readonly ConstructorInfo _bpmTimeProcessorCtor = AccessTools.FirstConstructor(typeof(BpmTimeProcessor), _ => true);
        private static readonly MethodInfo _createCustomBeatmapData = AccessTools.Method(typeof(BeatmapDataLoaderV2_6_0AndEarlierCustomify), nameof(CreateCustomBeatmapData));
        private static readonly MethodInfo _addCustomEvent = AccessTools.Method(typeof(BeatmapDataLoaderV2_6_0AndEarlierCustomify), nameof(AddCustomEvents));

        [HarmonyPrefix]
        [HarmonyPatch(nameof(BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.ConvertBeatmapSaveDataPreV2_5_0Inline))]
        private static bool CustomConvertPreV2_5_0(BeatmapSaveData beatmapSaveData)
        {
            Plugin.Log.Info("CONVERT PRE 2_5_0");
            if (beatmapSaveData is not Version2_6_0AndEarlierCustomBeatmapSaveData customBeatmapSaveData)
            {
                return true;
            }

            customBeatmapSaveData.ConvertBeatmapSaveDataPreV2_5_0();
            return false;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.GetBeatmapDataFromSaveDataJson))]
        private static IEnumerable<CodeInstruction> JsonTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(
                    false,
                    new CodeMatch(n =>
                        n.opcode == OpCodes.Call && ((MethodInfo)n.operand).Name == "FromJson"))
                .SetOperandAndAdvance(_deserialize)
                .InstructionEnumeration();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.GetBeatmapDataFromSaveData))]
        private static IEnumerable<CodeInstruction> Customify(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Newobj, _beatmapDataCtor))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .Set(OpCodes.Call, _createCustomBeatmapData)

                .MatchForward(false, new CodeMatch(OpCodes.Newobj, _bpmTimeProcessorCtor))
                .Advance(2)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldloc_S, 3),
                    new CodeInstruction(OpCodes.Ldloc_2),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, _addCustomEvent))

                // for reasons beyond my understanding, the leave will still take you to the InsertDefaultEnvironmentEvents, but inserting a nop fixes it...
                .End()
                .MatchBack(false, new CodeMatch(OpCodes.Leave))
                .Advance(1)
                .Insert(new CodeInstruction(OpCodes.Nop))

                .InstructionEnumeration();
        }

        private static BeatmapData CreateCustomBeatmapData(int numberOfLines, BeatmapSaveData beatmapSaveData)
        {
            if (beatmapSaveData is Version2_6_0AndEarlierCustomBeatmapSaveData customBeatmapSaveData)
            {
                return new CustomBeatmapData(
                    numberOfLines,
                    new CustomData(),
                    new CustomData(),
                    customBeatmapSaveData.customData,
                    BeatmapSaveDataHelpers.version2);
            }

            return new CustomBeatmapData(
                numberOfLines,
                new CustomData(),
                new CustomData(),
                new CustomData(),
                BeatmapSaveDataHelpers.version2);
        }

        private static void AddCustomEvents(
            BpmTimeProcessor timeProcessor,
            CustomBeatmapData beatmapData,
            BeatmapSaveData saveData)
        {
            if (saveData is not Version2_6_0AndEarlierCustomBeatmapSaveData customSaveData)
            {
                return;
            }

            foreach (Version2_6_0AndEarlierCustomBeatmapSaveData.CustomEventSaveData customEventSaveData in customSaveData.customEvents.OrderBy(n => n))
            {
                beatmapData.InsertCustomEventData(new CustomEventData(
                    timeProcessor.ConvertBeatToTime(customEventSaveData.beat),
                    customEventSaveData.type,
                    customEventSaveData.customData,
                    BeatmapSaveDataHelpers.version2));
            }
        }
    }
}
#endif
