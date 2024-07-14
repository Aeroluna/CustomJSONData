#if LATEST
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace CustomJSONData.HarmonyPatches
{
    [HarmonyPatch(typeof(BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader))]
    internal static class FixV2_6_0AndEarlierDefaultEnvironmentEvents
    {
        private static readonly MethodInfo _insertDefaultEvents = AccessTools.Method(
            typeof(DefaultEnvironmentEventsFactory), nameof(DefaultEnvironmentEventsFactory.InsertDefaultEvents));

        private static readonly MethodInfo _insertDefaultEventsConditional = AccessTools.Method(
            typeof(FixV2_6_0AndEarlierDefaultEnvironmentEvents), nameof(InsertDefaultEventsConditional));

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.GetBeatmapDataFromSaveData))]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Call, _insertDefaultEvents))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_1))
                .SetOperandAndAdvance(_insertDefaultEventsConditional)
                .InstructionEnumeration();
        }

        private static void InsertDefaultEventsConditional(BeatmapData beatmapData, bool flag3)
        {
            if (!flag3)
            {
                DefaultEnvironmentEventsFactory.InsertDefaultEvents(beatmapData);
            }
        }
    }
}
#endif
