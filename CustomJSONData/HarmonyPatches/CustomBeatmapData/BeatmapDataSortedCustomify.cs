using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using static HarmonyLib.AccessTools;

namespace CustomJSONData.HarmonyPatches
{
    [HarmonyPatch(typeof(BeatmapDataSortedListForTypeAndIds<BeatmapDataItem>))]
    internal static class BeatmapDataSortedCustomify
    {
        private static readonly MethodInfo _getType = Method(typeof(object), nameof(GetType));
        private static readonly MethodInfo _getCustomType = Method(typeof(CustomBeatmapData), nameof(CustomBeatmapData.GetCustomType));

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(BeatmapDataSortedListForTypeAndIds<BeatmapDataItem>.InsertItem))]
        private static IEnumerable<CodeInstruction> CustomifyInsertItem(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .CustomifyGetType()
                .InstructionEnumeration();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(BeatmapDataSortedListForTypeAndIds<BeatmapDataItem>.RemoveItem))]
        private static IEnumerable<CodeInstruction> CustomifyRemoveItem(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .CustomifyGetType()
                .InstructionEnumeration();
        }

        private static CodeMatcher CustomifyGetType(this CodeMatcher matcher)
        {
            return matcher
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _getType))
                .Set(OpCodes.Call, _getCustomType);
        }
    }
}
