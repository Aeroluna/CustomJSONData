using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;

namespace CustomJSONData.HarmonyPatches
{
    [HarmonyPatch(typeof(BurstSliderSpawner))]
    internal static class BurstSliderSpawnerCustomify
    {
        private static readonly MethodInfo _original = AccessTools.Method(typeof(NoteData), nameof(NoteData.CreateBurstSliderNoteData));
        private static readonly MethodInfo _custom = AccessTools.Method(typeof(CustomNoteData), nameof(CustomNoteData.CreateCustomBurstSliderNoteData));
        private static readonly MethodInfo _getData = AccessTools.Method(typeof(BurstSliderSpawnerCustomify), nameof(GetSliderData));

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(BurstSliderSpawner.ProcessSliderData))]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Call, _original))
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, _getData))
                .SetOperandAndAdvance(_custom)
                .InstructionEnumeration();
        }

        private static CustomData GetSliderData(CustomSliderData sliderData)
        {
            return sliderData.customData;
        }
    }
}
