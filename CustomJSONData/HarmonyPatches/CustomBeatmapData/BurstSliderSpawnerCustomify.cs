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
        private static readonly MethodInfo _custom = AccessTools.Method(typeof(BurstSliderSpawnerCustomify), nameof(CreateCustomBurstSliderNoteData));

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(BurstSliderSpawner.ProcessSliderData))]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Call, _original))
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0))
                .SetOperandAndAdvance(_custom)
                .InstructionEnumeration();
        }

        private static NoteData CreateCustomBurstSliderNoteData(
            float time,
#if LATEST
            float beat,
            int rotation,
#endif
            int lineIndex,
            NoteLineLayer noteLineLayer,
            NoteLineLayer beforeJumpNoteLineLayer,
            ColorType colorType,
            NoteCutDirection cutDirection,
            float cutSfxVolumeMultiplier,
            SliderData sliderData)
        {
            if (sliderData is CustomSliderData customSliderData)
            {
                return CustomNoteData.CreateCustomBurstSliderNoteData(
                    time,
#if LATEST
                    beat,
                    rotation,
#endif
                    lineIndex,
                    noteLineLayer,
                    beforeJumpNoteLineLayer,
                    colorType,
                    cutDirection,
                    cutSfxVolumeMultiplier,
                    customSliderData.customData);
            }

            return NoteData.CreateBurstSliderNoteData(
                time,
#if LATEST
                beat,
                rotation,
#endif
                lineIndex,
                noteLineLayer,
                beforeJumpNoteLineLayer,
                colorType,
                cutDirection,
                cutSfxVolumeMultiplier);
        }
    }
}
