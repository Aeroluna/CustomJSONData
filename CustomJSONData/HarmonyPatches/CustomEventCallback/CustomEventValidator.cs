using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;

namespace CustomJSONData.HarmonyPatches
{
    [HarmonyPatch(typeof(BeatmapCallbacksController))]
    internal static class CustomEventValidator
    {
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(BeatmapCallbacksController.ManualUpdate))]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(
                    false,
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldc_I4_1),
                    new CodeMatch(OpCodes.Beq));
            CodeInstruction[] duped = matcher.InstructionsWithOffsets(0, 4).Select(n => new CodeInstruction(n)).ToArray();
            duped[2] = new CodeInstruction(OpCodes.Ldc_I4_2);
            return matcher.InsertAndAdvance(duped)

                // cursed transpiler bug
                // https://github.com/BepInEx/HarmonyX/issues/65
                .End()
                .MatchBack(false, new CodeMatch(OpCodes.Leave))
                .Advance(1)
                .Insert(new CodeInstruction(OpCodes.Nop))

                .InstructionEnumeration();
        }
    }
}
