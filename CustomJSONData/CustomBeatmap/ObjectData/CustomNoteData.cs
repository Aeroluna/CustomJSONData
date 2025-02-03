using System;

namespace CustomJSONData.CustomBeatmap
{
    public class CustomNoteData : NoteData, ICustomData, IVersionable
    {
        public CustomNoteData(
            float time,
#if !PRE_V1_39_1
            float beat,
            int rotation,
#endif
            int lineIndex,
            NoteLineLayer noteLineLayer,
            NoteLineLayer beforeJumpNoteLineLayer,
            GameplayType gameplayType,
            ScoringType scoringType,
            ColorType colorType,
            NoteCutDirection cutDirection,
            float timeToNextColorNote,
            float timeToPrevColorNote,
            int flipLineIndex,
            float flipYSide,
            float cutDirectionAngleOffset,
            float cutSfxVolumeMultiplier,
            CustomData customData,
            Version version)
            : base(
                time,
#if !PRE_V1_39_1
                beat,
                rotation,
#endif
                lineIndex,
                noteLineLayer,
                beforeJumpNoteLineLayer,
                gameplayType,
                scoringType,
                colorType,
                cutDirection,
                timeToNextColorNote,
                timeToPrevColorNote,
                flipLineIndex,
                flipYSide,
                cutDirectionAngleOffset,
                cutSfxVolumeMultiplier)
        {
            this.customData = customData;
            this.version = version;
        }

        public CustomData customData { get; }

        public Version version { get; }

        public static CustomNoteData CreateCustomBasicNoteData(
            float time,
#if !PRE_V1_39_1
            float beat,
            int rotation,
#endif
            int lineIndex,
            NoteLineLayer noteLineLayer,
            ColorType colorType,
            NoteCutDirection cutDirection,
            CustomData customData,
            Version version)
        {
            return new CustomNoteData(
                time,
#if !PRE_V1_39_1
                beat,
                rotation,
#endif
                lineIndex,
                noteLineLayer,
                noteLineLayer,
                GameplayType.Normal,
                ScoringType.Normal,
                colorType,
                cutDirection,
                0f,
                0f,
                lineIndex,
                0f,
                0f,
                1f,
                customData,
                version);
        }

        public static CustomNoteData CreateCustomBombNoteData(
            float time,
#if !PRE_V1_39_1
            float beat,
            int rotation,
#endif
            int lineIndex,
            NoteLineLayer noteLineLayer,
            CustomData customData,
            Version version)
        {
            return new CustomNoteData(
                time,
#if !PRE_V1_39_1
                beat,
                rotation,
#endif
                lineIndex,
                noteLineLayer,
                noteLineLayer,
                GameplayType.Bomb,
                ScoringType.NoScore,
                ColorType.None,
                NoteCutDirection.None,
                0f,
                0f,
                lineIndex,
                0f,
                0f,
                1f,
                customData,
                version);
        }

        public static CustomNoteData CreateCustomBurstSliderNoteData(
            float time,
#if !PRE_V1_39_1
            float beat,
            int rotation,
#endif
            int lineIndex,
            NoteLineLayer noteLineLayer,
            NoteLineLayer beforeJumpNoteLineLayer,
            ColorType colorType,
            NoteCutDirection cutDirection,
            float cutSfxVolumeMultiplier,
            CustomData customData)
        {
            return new CustomNoteData(
                time,
#if !PRE_V1_39_1
                beat,
                rotation,
#endif
                lineIndex,
                noteLineLayer,
                beforeJumpNoteLineLayer,
                GameplayType.BurstSliderElement,
#if LATEST
                ScoringType.ChainLink,
#else
                ScoringType.BurstSliderElement,
#endif
                colorType,
                cutDirection,
                0f,
                0f,
                lineIndex,
                0f,
                0f,
                cutSfxVolumeMultiplier,
                customData,
                VersionExtensions.version3);
        }

        public override BeatmapDataItem GetCopy()
        {
            return new CustomNoteData(
                time,
#if !PRE_V1_39_1
                beat,
                rotation,
#endif
                lineIndex,
                noteLineLayer,
                beforeJumpNoteLineLayer,
                gameplayType,
                scoringType,
                colorType,
                cutDirection,
                timeToNextColorNote,
                timeToPrevColorNote,
                flipLineIndex,
                flipYSide,
                cutDirectionAngleOffset,
                cutSfxVolumeMultiplier,
                customData.Copy(),
                version);
        }
    }
}
