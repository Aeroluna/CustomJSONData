#if !PRE_V1_37_1

namespace CustomJSONData.CustomBeatmap
{
    // BeatmapLevel is override by playlist stuff so we cant both lay claim to it
    /*public class CustomBeatmapLevel : BeatmapLevel, ICustomData
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
    }*/

    public class CustomBeatmapBasicData : BeatmapBasicData
    {
        public CustomBeatmapBasicData(
            float noteJumpMovementSpeed,
            float noteJumpStartBeatOffset,
            EnvironmentName environmentName,
            ColorScheme beatmapColorScheme,
            int notesCount,
#if !PRE_V1_39_1
            int cuttableObjectsCount,
#endif
            int obstaclesCount,
            int bombsCount,
            string[] mappers,
            string[] lighters,
            CustomData levelCustomData,
            CustomData beatmapCustomData)
            : base(
                noteJumpMovementSpeed,
                noteJumpStartBeatOffset,
                environmentName,
                beatmapColorScheme,
                notesCount,
#if !PRE_V1_39_1
                cuttableObjectsCount,
#endif
                obstaclesCount,
                bombsCount,
                mappers,
                lighters)
        {
            this.levelCustomData = levelCustomData;
            this.beatmapCustomData = beatmapCustomData;
        }

        public CustomData levelCustomData { get; }

        public CustomData beatmapCustomData { get; }
    }
}
#endif
