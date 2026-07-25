using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using JetBrains.Annotations;

namespace CustomJSONData.HarmonyPatches
{
#if LATEST
    [HarmonyPatch(
        typeof(BeatmapDataItem),
        MethodType.Constructor,
        typeof(float),
        typeof(int),
        typeof(int),
        typeof(BeatmapDataItem.BeatmapDataItemType))]
#else
    [HarmonyPatch(typeof(CallbacksInTime), nameof(CallbacksInTime.CallCallbacks), typeof(BeatmapDataItem))]
#endif
    internal static class CallbackCustomTypeGetter
    {
        private static readonly MethodInfo _getType = AccessTools.Method(typeof(object), nameof(GetType));

        private static readonly MethodInfo _getCustomType = AccessTools.Method(
            typeof(CustomBeatmapData),
            nameof(CustomBeatmapData.GetCustomType));

        [UsedImplicitly]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
#if LATEST
                .MatchForward(false, new CodeMatch(OpCodes.Call, _getType))
#else
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _getType))
#endif
                .Set(OpCodes.Call, _getCustomType)
                .InstructionEnumeration();
        }
    }
}
