#if LATEST
using System.Collections.Generic;

namespace CustomJSONData.CustomBeatmap
{
    public class CustomBeatmapLevel : BeatmapLevel, ICustomData
    {
        public CustomBeatmapLevel(
            int version,
            bool hasPrecalculatedData,
            string levelID,
            string songName,
            string songSubName,
            string songAuthorName,
            string[] allMappers,
            string[] allLighters,
            float beatsPerMinute,
            float integratedLufs,
            float songTimeOffset,
            float previewStartTime,
            float previewDuration,
            float songDuration,
            PlayerSensitivityFlag contentRating,
            IPreviewMediaData previewMediaData,
            IReadOnlyDictionary<(BeatmapCharacteristicSO BeatmapCharacteristicSO, BeatmapDifficulty BeatmapDifficulty), BeatmapBasicData> beatmapBasicData,
            CustomData customData)
            : base(
                version,
                hasPrecalculatedData,
                levelID,
                songName,
                songSubName,
                songAuthorName,
                allMappers,
                allLighters,
                beatsPerMinute,
                integratedLufs,
                songTimeOffset,
                previewStartTime,
                previewDuration,
                songDuration,
                contentRating,
                previewMediaData,
                beatmapBasicData)
        {
            this.customData = customData;
        }

        public CustomData customData { get; }
    }

    public class CustomBeatmapBasicData : BeatmapBasicData, ICustomData
    {
        public CustomBeatmapBasicData(
            float noteJumpMovementSpeed,
            float noteJumpStartBeatOffset,
            EnvironmentName environmentName,
            ColorScheme beatmapColorScheme,
            int notesCount,
            int obstaclesCount,
            int bombsCount,
            string[] mappers,
            string[] lighters,
            CustomData customData)
            : base(
                noteJumpMovementSpeed,
                noteJumpStartBeatOffset,
                environmentName,
                beatmapColorScheme,
                notesCount,
                obstaclesCount,
                bombsCount,
                mappers,
                lighters)
        {
            this.customData = customData;
        }

        public CustomData customData { get; }
    }
}
#endif
