#if LATEST
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;

namespace CustomJSONData.HarmonyPatches
{
    [HarmonyPatch]
    internal static class CustomLevelLoaderCustomify
    {
        private static readonly ConstructorInfo _fileDifficultyBeatmapCtor = AccessTools.FirstConstructor(typeof(FileDifficultyBeatmap), _ => true);
        private static readonly ConstructorInfo _fileSystemBeatmapLevelDataCtor = AccessTools.FirstConstructor(typeof(FileSystemBeatmapLevelData), _ => true);

        private static readonly ConstructorInfo _customFileDifficultyBeatmapCtor = AccessTools.FirstConstructor(typeof(CustomFileDifficultyBeatmap), _ => true);
        private static readonly ConstructorInfo _customFileBeatmapLevelDataCtor = AccessTools.FirstConstructor(typeof(CustomFileBeatmapLevelData), _ => true);

        ////private static readonly ConstructorInfo _beatmapLevelCtor = AccessTools.FirstConstructor(typeof(BeatmapLevel), _ => true);
        private static readonly ConstructorInfo _beatmapBasicDataCtor = AccessTools.FirstConstructor(typeof(BeatmapBasicData), _ => true);

        ////private static readonly ConstructorInfo _customBeatmapLevelCtor = AccessTools.FirstConstructor(typeof(CustomBeatmapLevel), _ => true);
        private static readonly ConstructorInfo _customBeatmapBasicDataCtor = AccessTools.FirstConstructor(typeof(CustomBeatmapBasicData), _ => true);

        private static readonly MethodInfo _getDifficultyBeatmapCustomData =
            AccessTools.Method(typeof(CustomLevelLoaderCustomify), nameof(GetDifficultyBeatmapCustomData));

        private static readonly MethodInfo _getLevelInfoCustomData =
            AccessTools.Method(typeof(CustomLevelLoaderCustomify), nameof(GetLevelInfoCustomData));

        private static CustomData GetDifficultyBeatmapCustomData(
            StandardLevelInfoSaveData.DifficultyBeatmap difficultyBeatmap)
        {
            return difficultyBeatmap is CustomLevelInfoSaveData.DifficultyBeatmap customDifficultyBeatmap
                ? customDifficultyBeatmap.customData
                : new CustomData();
        }

        private static CustomData GetLevelInfoCustomData(
            StandardLevelInfoSaveData standardLevelInfoSaveData)
        {
            return standardLevelInfoSaveData is CustomLevelInfoSaveData customLevelInfo
                ? customLevelInfo.customData
                : new CustomData();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(CustomLevelLoader), nameof(CustomLevelLoader.CreateBeatmapLevelDataFromV3))]
        private static IEnumerable<CodeInstruction> BeatmapDataTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Newobj, _fileDifficultyBeatmapCtor))
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldloc_S, 7),
                    new CodeInstruction(OpCodes.Call, _getDifficultyBeatmapCustomData))
                .SetOperandAndAdvance(_customFileDifficultyBeatmapCtor)

                .MatchForward(false, new CodeMatch(OpCodes.Newobj, _fileSystemBeatmapLevelDataCtor))
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_2),
                    new CodeInstruction(OpCodes.Call, _getLevelInfoCustomData))
                .SetOperandAndAdvance(_customFileBeatmapLevelDataCtor)

                .InstructionEnumeration();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(CustomLevelLoader), nameof(CustomLevelLoader.CreateBeatmapLevelFromV3))]
        private static IEnumerable<CodeInstruction> BeatmapLevelTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Newobj, _beatmapBasicDataCtor))
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_2),
                    new CodeInstruction(OpCodes.Call, _getLevelInfoCustomData),
                    new CodeInstruction(OpCodes.Ldloc_S, 16),
                    new CodeInstruction(OpCodes.Call, _getDifficultyBeatmapCustomData))
                .SetOperandAndAdvance(_customBeatmapBasicDataCtor)

                .InstructionEnumeration();
        }
    }
}
#endif
