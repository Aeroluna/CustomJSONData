﻿#if PRE_V1_37_1
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BeatmapSaveDataVersion3;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;

namespace CustomJSONData.HarmonyPatches
{
    // Create a CustomBeatmapSaveData instead of a BeatmapSaveData
    [HarmonyPatch(typeof(CustomLevelLoader))]
    internal static class CustomLevelLoaderRedirect
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(CustomLevelLoader.LoadBeatmapDataBasicInfo))]
        private static bool Prefix(
            ref Tuple<BeatmapSaveData, BeatmapDataBasicInfo> __result,
            string customLevelPath,
            string difficultyFileName,
            StandardLevelInfoSaveData standardLevelInfoSaveData)
        {
            string path = Path.Combine(customLevelPath, difficultyFileName);
            if (!File.Exists(path))
            {
                return false;
            }

            CustomData beatmapData;
            CustomData levelData;
            if (standardLevelInfoSaveData is CustomLevelInfoSaveData customLevelInfoSaveData)
            {
                IEnumerable<StandardLevelInfoSaveData.DifficultyBeatmap> difficultyBeatmaps = customLevelInfoSaveData.difficultyBeatmapSets.SelectMany(n => n.difficultyBeatmaps);
                beatmapData = ((CustomLevelInfoSaveData.DifficultyBeatmap)difficultyBeatmaps.First(n => n.beatmapFilename == difficultyFileName)).customData;
                levelData = customLevelInfoSaveData.customData;
            }
            else
            {
                beatmapData = new CustomData();
                levelData = new CustomData();
            }

            try
            {
                Version3CustomBeatmapSaveData saveData = Version3CustomBeatmapSaveData.Deserialize(path, beatmapData, levelData);
                __result = new Tuple<BeatmapSaveData, BeatmapDataBasicInfo>(
                    saveData,
                    BeatmapDataLoader.GetBeatmapDataBasicInfoFromSaveData(saveData));
            }
            catch
            {
                Plugin.Log.Critical($"Exception while deserializing [{path}]");
                throw;
            }

            return false;
        }
    }
}
#endif
