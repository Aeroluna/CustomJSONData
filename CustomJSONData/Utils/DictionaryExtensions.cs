using System.Linq;
using CustomJSONData.CustomBeatmap;
using JetBrains.Annotations;

namespace CustomJSONData
{
    public static class DictionaryExtensions
    {
#if LATEST
        [PublicAPI]
        public static CustomData GetBeatmapCustomData(this IBeatmapLevelData beatmapLevelData, in BeatmapKey beatmapKey)
        {
            // ReSharper disable once InvertIf
            if (beatmapLevelData is CustomFileBeatmapLevelData customFileBeatmapLevelData)
            {
                FileDifficultyBeatmap? fileDifficultyBeatmap =
                    customFileBeatmapLevelData.GetDifficultyBeatmap(beatmapKey);
                if (fileDifficultyBeatmap is CustomFileDifficultyBeatmap customFileDifficultyBeatmap)
                {
                    return customFileDifficultyBeatmap.customData;
                }
            }

            return new CustomData();
        }

        [PublicAPI]
        public static CustomData GetLevelCustomData(this IBeatmapLevelData beatmapLevelData)
        {
            return beatmapLevelData is CustomFileBeatmapLevelData customFileBeatmapLevelData
                ? customFileBeatmapLevelData.customData : new CustomData();
        }

        [PublicAPI]
        public static CustomData GetBeatmapCustomData(this BeatmapLevel beatmapLevel, in BeatmapKey beatmapKey)
        {
            BeatmapBasicData? beatmapBasicData =
                beatmapLevel.GetDifficultyBeatmapData(beatmapKey.beatmapCharacteristic, beatmapKey.difficulty);
            if (beatmapBasicData is CustomBeatmapBasicData customBeatmapBasicData)
            {
                return customBeatmapBasicData.beatmapCustomData;
            }

            return new CustomData();
        }

        [PublicAPI]
        public static CustomData GetLevelCustomData(this BeatmapLevel beatmapLevel)
        {
            // a lil jank but w/e
            BeatmapBasicData? beatmapBasicData = beatmapLevel.beatmapBasicData.Values.FirstOrDefault();
            if (beatmapBasicData is CustomBeatmapBasicData customBeatmapBasicData)
            {
                return customBeatmapBasicData.levelCustomData;
            }

            return new CustomData();
        }
#else
        [PublicAPI]
        public static Version3CustomBeatmapSaveData? GetBeatmapSaveData(this IDifficultyBeatmap difficultyBeatmap)
        {
            return difficultyBeatmap is CustomDifficultyBeatmap { beatmapSaveData: Version3CustomBeatmapSaveData customBeatmapSaveData }
                ? customBeatmapSaveData : null;
        }

        [PublicAPI]
        public static CustomData GetBeatmapCustomData(this IDifficultyBeatmap difficultyBeatmap)
        {
            return difficultyBeatmap is CustomDifficultyBeatmap { beatmapSaveData: Version3CustomBeatmapSaveData customBeatmapSaveData }
                ? customBeatmapSaveData.beatmapCustomData : new CustomData();
        }

        [PublicAPI]
        public static CustomData GetLevelCustomData(this IDifficultyBeatmap difficultyBeatmap)
        {
            return difficultyBeatmap is CustomDifficultyBeatmap { beatmapSaveData: Version3CustomBeatmapSaveData customBeatmapSaveData }
                ? customBeatmapSaveData.levelCustomData : new CustomData();
        }
#endif
    }
}
