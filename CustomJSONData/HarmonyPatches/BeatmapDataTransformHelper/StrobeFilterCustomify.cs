using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;

namespace CustomJSONData.HarmonyPatches
{
    [HarmonyPatch(typeof(BeatmapDataStrobeFilterTransform))]
    internal static class StrobeFilterCustomify
    {
        private static readonly ConstructorInfo _beatmapDataCtor = AccessTools.FirstConstructor(typeof(BeatmapData), _ => true);
        private static readonly MethodInfo _newCustomBeatmapData = AccessTools.Method(typeof(StrobeFilterCustomify), nameof(NewCustomBeatmapData));

        private static readonly MethodInfo _insertCustomEvent = AccessTools.Method(typeof(StrobeFilterCustomify), nameof(InsertCustomEvent));

        private static readonly ConstructorInfo _eventDataCtor = AccessTools.FirstConstructor(typeof(BasicBeatmapEventData), _ => true);
        private static readonly MethodInfo _createEventData = AccessTools.Method(typeof(StrobeFilterCustomify), nameof(CreateCustomBeatmapEventData));

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(BeatmapDataStrobeFilterTransform.CreateTransformedData))]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(
                    false,
                    new CodeMatch(OpCodes.Newobj, _beatmapDataCtor))
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0))
                .Set(OpCodes.Call, _newCustomBeatmapData)

                .MatchForward(
                    false,
                    new CodeMatch(OpCodes.Isinst))
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldloc_0),
                    new CodeInstruction(OpCodes.Call, _insertCustomEvent))

                .MatchForward(
                    false,
                    new CodeMatch(OpCodes.Newobj, _eventDataCtor))
                .Repeat(n => n
                    .InsertAndAdvance(
#pragma warning disable SA1114
#if LATEST
                        new CodeInstruction(OpCodes.Ldloc_S, 7))
#else
                        new CodeInstruction(OpCodes.Ldloc_S, 5))
#endif
#pragma warning restore SA1114
                    .SetAndAdvance(OpCodes.Call, _createEventData))
                .InstructionEnumeration();
        }

        private static BeatmapData NewCustomBeatmapData(int numberOfLines, BeatmapData beatmapData)
        {
            if (beatmapData is CustomBeatmapData customBeatmapData)
            {
                return new CustomBeatmapData(
                    numberOfLines,
                    customBeatmapData.beatmapCustomData.Copy(),
                    customBeatmapData.levelCustomData.Copy(),
                    customBeatmapData.customData.Copy(),
                    customBeatmapData.version);
            }

            return new BeatmapData(numberOfLines);
        }

        private static BeatmapDataItem InsertCustomEvent(BeatmapDataItem beatmapDataItem, BeatmapData beatmapData)
        {
            if (beatmapData is CustomBeatmapData customBeatmapData &&
                beatmapDataItem is CustomEventData customEventData)
            {
                customBeatmapData.InsertCustomEventDataInOrder(customEventData);
            }

            return beatmapDataItem; // return to stack
        }

        private static BeatmapEventData CreateCustomBeatmapEventData(
            float time,
            BasicBeatmapEventType basicBeatmapEventType,
            int value,
            float floatValue,
            BeatmapEventData beatmapEventData)
        {
            if (beatmapEventData is CustomBasicBeatmapEventData customBeatmapEventData)
            {
                return new CustomBasicBeatmapEventData(
                    time,
                    basicBeatmapEventType,
                    value,
                    floatValue,
                    customBeatmapEventData.customData,
                    customBeatmapEventData.version);
            }

            return new BasicBeatmapEventData(
                time,
                basicBeatmapEventType,
                value,
                floatValue);
        }
    }
}
