using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;

namespace CustomJSONData.HarmonyPatches
{
    [HarmonyPatch(typeof(DefaultEnvironmentEventsFactory))]
    internal static class DefaultEnvironmentEventsCustomify
    {
        private static readonly ConstructorInfo _eventDataCtor = AccessTools.FirstConstructor(typeof(BasicBeatmapEventData), _ => true);
        private static readonly ConstructorInfo _customEventDataCtor = AccessTools.FirstConstructor(typeof(CustomBasicBeatmapEventData), _ => true);
        private static readonly ConstructorInfo _customDataCtor = AccessTools.Constructor(typeof(CustomData));

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
                        new CodeInstruction(OpCodes.Newobj, _customDataCtor))
                    .SetOperandAndAdvance(_customEventDataCtor))
                .InstructionEnumeration();
        }
    }
}
