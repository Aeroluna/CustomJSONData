#if !PRE_V1_37_1
using System.Collections.Generic;

namespace CustomJSONData.CustomBeatmap
{
    public class CustomFileBeatmapLevelData : FileSystemBeatmapLevelData, ICustomData
    {
        public CustomFileBeatmapLevelData(
            string name,
            string audioClipPath,
            string audioDataPath,
#if LATEST
            Dictionary<(BeatmapCharacteristic Characteristic, BeatmapDifficulty Difficulty), FileDifficultyBeatmap> difficultyBeatmaps,
#else
            Dictionary<(BeatmapCharacteristicSO Characteristic, BeatmapDifficulty Difficulty), FileDifficultyBeatmap> difficultyBeatmaps,
#endif
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
