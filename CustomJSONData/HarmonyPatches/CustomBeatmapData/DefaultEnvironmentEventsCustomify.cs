using CustomJSONData.CustomBeatmap;
using HarmonyLib;
#if PRE_V1_37_1
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
#endif

namespace CustomJSONData.HarmonyPatches
{
    [HarmonyPatch(typeof(DefaultEnvironmentEventsFactory))]
    internal static class DefaultEnvironmentEventsCustomify
    {
#if !PRE_V1_37_1
        [HarmonyPrefix]
        [HarmonyPatch(nameof(DefaultEnvironmentEventsFactory.InsertDefaultEvents))]
        private static bool Prefix(BeatmapData beatmapData)
        {
            // extra check to not add custom variants to non-custom beatmapdata such as v4 maps
            if (beatmapData is not CustomBeatmapData customBeatmapData)
            {
                return true;
            }

            beatmapData.InsertBeatmapEventData(new CustomBasicBeatmapEventData(0, BasicBeatmapEventType.Event0, 1, 1, new CustomData(), customBeatmapData.version));
            beatmapData.InsertBeatmapEventData(new CustomBasicBeatmapEventData(0, BasicBeatmapEventType.Event4, 1, 1, new CustomData(), customBeatmapData.version));

            return false;
        }
#else
        private static readonly ConstructorInfo _eventDataCtor = AccessTools.FirstConstructor(typeof(BasicBeatmapEventData), _ => true);
        private static readonly ConstructorInfo _customEventDataCtor = AccessTools.FirstConstructor(typeof(CustomBasicBeatmapEventData), _ => true);
        private static readonly ConstructorInfo _customDataCtor = AccessTools.Constructor(typeof(CustomData));

        private static readonly MethodInfo _version3 = AccessTools.PropertyGetter(typeof(VersionExtensions), nameof(VersionExtensions.version3));

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(DefaultEnvironmentEventsFactory.InsertDefaultEnvironmentEvents))]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(
                    false,
                    new CodeMatch(OpCodes.Newobj, _eventDataCtor))
                .Repeat(n => n
                    .InsertAndAdvance(
                        new CodeInstruction(OpCodes.Newobj, _customDataCtor),
                        new CodeInstruction(OpCodes.Call, _version3))
                    .SetOperandAndAdvance(_customEventDataCtor))
                .InstructionEnumeration();
        }
#endif
    }
}
