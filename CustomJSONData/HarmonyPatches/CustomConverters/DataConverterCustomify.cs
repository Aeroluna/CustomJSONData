using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using static HarmonyLib.AccessTools;

namespace CustomJSONData.HarmonyPatches.CustomConverters
{
    [HarmonyPatch(typeof(DataConvertor<BeatmapDataItem>))]
    internal static class DataConverterCustomify
    {
        private static readonly MethodInfo _getType = Method(typeof(object), nameof(GetType));
        private static readonly MethodInfo _getCustomType = Method(typeof(CustomBeatmapData), nameof(CustomBeatmapData.GetCustomType));

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(DataConvertor<BeatmapDataItem>.ProcessItem))]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _getType))
                .Set(OpCodes.Call, _getCustomType)
                .InstructionEnumeration();
        }
    }
}
