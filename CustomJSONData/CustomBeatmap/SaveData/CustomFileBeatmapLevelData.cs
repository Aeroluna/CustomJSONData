#if LATEST
using System.Collections.Generic;

namespace CustomJSONData.CustomBeatmap
{
    public class CustomFileBeatmapLevelData : FileSystemBeatmapLevelData, ICustomData
    {
        public CustomFileBeatmapLevelData(
            string name,
            string audioClipPath,
            string audioDataPath,
            Dictionary<(BeatmapCharacteristicSO Characteristic, BeatmapDifficulty Difficulty), FileDifficultyBeatmap> difficultyBeatmaps,
            CustomData customData)
            : base(name, audioClipPath, audioDataPath, difficultyBeatmaps)
        {
            this.customData = customData;
        }

        public CustomData customData { get; }
    }

    public class CustomFileDifficultyBeatmap : FileDifficultyBeatmap, ICustomData
    {
        public CustomFileDifficultyBeatmap(string beatmapPath, string lightshowPath, CustomData customData)
            : base(beatmapPath, lightshowPath)
        {
            this.customData = customData;
        }

        public CustomData customData { get; }
    }
}
#endif
