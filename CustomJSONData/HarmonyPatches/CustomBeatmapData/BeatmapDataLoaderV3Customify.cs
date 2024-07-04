#if LATEST
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BeatmapSaveDataVersion3;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;

namespace CustomJSONData.HarmonyPatches
{
    [HarmonyPatch(typeof(BeatmapDataLoaderVersion3.BeatmapDataLoader))]
    internal static class BeatmapDataLoaderV3Customify
    {
        private static readonly MethodInfo _deserialize = AccessTools.Method(typeof(Version3CustomBeatmapSaveData), nameof(Version3CustomBeatmapSaveData.Deserialize));

        private static readonly ConstructorInfo _beatmapDataCtor = AccessTools.FirstConstructor(typeof(BeatmapData), _ => true);
        private static readonly ConstructorInfo _bpmChangeCtor = AccessTools.FirstConstructor(typeof(BPMChangeBeatmapEventData), _ => true);
        private static readonly MethodInfo _createCustomBeatmapData = AccessTools.Method(typeof(BeatmapDataLoaderV3Customify), nameof(CreateCustomBeatmapData));
        private static readonly MethodInfo _createCustomBPMChangeData = AccessTools.Method(typeof(BeatmapDataLoaderV3Customify), nameof(CreateCustomBPMChangeData));
        private static readonly MethodInfo _addCustomEvent = AccessTools.Method(typeof(BeatmapDataLoaderV3Customify), nameof(AddCustomEvents));

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(BeatmapDataLoaderVersion3.BeatmapDataLoader.GetBeatmapDataFromSaveDataJson))]
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
        [HarmonyPatch(nameof(BeatmapDataLoaderVersion3.BeatmapDataLoader.GetBeatmapDataFromSaveData))]
        private static IEnumerable<CodeInstruction> Customify(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Newobj, _beatmapDataCtor))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .Set(OpCodes.Call, _createCustomBeatmapData)

                .MatchForward(false, new CodeMatch(OpCodes.Newobj))
                .Advance(2)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldloc_S, 4),
                    new CodeInstruction(OpCodes.Ldloc_2),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, _addCustomEvent))

                .MatchForward(false, new CodeMatch(OpCodes.Newobj, _bpmChangeCtor))
                .Set(OpCodes.Call, _createCustomBPMChangeData)

                // for reasons beyond my understanding, the leave will still take you to the InsertDefaultEnvironmentEvents, but inserting a nop fixes it...
                .End()
                .MatchBack(false, new CodeMatch(OpCodes.Leave))
                .Advance(1)
                .Insert(new CodeInstruction(OpCodes.Nop))

                .InstructionEnumeration();
        }

        private static BeatmapData CreateCustomBeatmapData(int numberOfLines, BeatmapSaveData beatmapSaveData)
        {
            if (beatmapSaveData is Version3CustomBeatmapSaveData customBeatmapSaveData)
            {
                return new CustomBeatmapData(
                    numberOfLines,
                    new CustomData(),
                    new CustomData(),
                    customBeatmapSaveData.customData,
                    BeatmapSaveDataHelpers.version3);
            }

            return new CustomBeatmapData(
                numberOfLines,
                new CustomData(),
                new CustomData(),
                new CustomData(),
                BeatmapSaveDataHelpers.version3);
        }

        private static BPMChangeBeatmapEventData CreateCustomBPMChangeData(float time, float bpm)
        {
            return new CustomBPMChangeBeatmapEventData(
                time,
                bpm,
                new CustomData(),
                BeatmapSaveDataHelpers.version3);
        }

        private static void AddCustomEvents(
            BpmTimeProcessor timeProcessor,
            CustomBeatmapData beatmapData,
            BeatmapSaveData saveData)
        {
            if (saveData is not Version3CustomBeatmapSaveData customSaveData)
            {
                return;
            }

            foreach (Version3CustomBeatmapSaveData.CustomEventSaveData customEventSaveData in customSaveData.customEvents.OrderBy(n => n))
            {
                beatmapData.InsertCustomEventData(new CustomEventData(
                    timeProcessor.ConvertBeatToTime(customEventSaveData.beat),
                    customEventSaveData.type,
                    customEventSaveData.customData,
                    BeatmapSaveDataHelpers.version3));
            }
        }
    }
}
#endif
