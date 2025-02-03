using System;

namespace CustomJSONData.CustomBeatmap
{
    public class CustomSliderData : SliderData, ICustomData, IVersionable
    {
        public CustomSliderData(
            Type sliderType,
            ColorType colorType,
            bool hasHeadNote,
            float headTime,
#if !PRE_V1_39_1
            float headBeat,
            int rotation,
#endif
            int headLineIndex,
            NoteLineLayer headLineLayer,
            NoteLineLayer headBeforeJumpLineLayer,
            float headControlPointLengthMultiplier,
            NoteCutDirection headCutDirection,
            float headCutDirectionAngleOffset,
            bool hasTailNote,
            float tailTime,
#if !PRE_V1_39_1
            int tailRotation,
#endif
            int tailLineIndex,
            NoteLineLayer tailLineLayer,
            NoteLineLayer tailBeforeJumpLineLayer,
            float tailControlPointLengthMultiplier,
            NoteCutDirection tailCutDirection,
            float tailCutDirectionAngleOffset,
            SliderMidAnchorMode midAnchorMode,
            int sliceCount,
            float squishAmount,
            CustomData customData,
            Version version)
            : base(
                sliderType,
                colorType,
                hasHeadNote,
                headTime,
#if !PRE_V1_39_1
                headBeat,
                rotation,
#endif
                headLineIndex,
                headLineLayer,
                headBeforeJumpLineLayer,
                headControlPointLengthMultiplier,
                headCutDirection,
                headCutDirectionAngleOffset,
                hasTailNote,
                tailTime,
#if !PRE_V1_39_1
                tailRotation,
#endif
                tailLineIndex,
                tailLineLayer,
                tailBeforeJumpLineLayer,
                tailControlPointLengthMultiplier,
                tailCutDirection,
                tailCutDirectionAngleOffset,
                midAnchorMode,
                sliceCount,
                squishAmount)
        {
            this.customData = customData;
            this.version = version;
        }

        public CustomData customData { get; }

        public Version version { get; }

        public static SliderData CreateCustomSliderData(
            ColorType colorType,
            float headTime,
#if !PRE_V1_39_1
            float headBeat,
            int rotation,
#endif
            int headLineIndex,
            NoteLineLayer headLineLayer,
            NoteLineLayer headBeforeJumpLineLayer,
            float headControlPointLengthMultiplier,
            NoteCutDirection headCutDirection,
            float tailTime,
#if !PRE_V1_39_1
            int tailRotation,
#endif
            int tailLineIndex,
            NoteLineLayer tailLineLayer,
            NoteLineLayer tailBeforeJumpLineLayer,
            float tailControlPointLengthMultiplier,
            NoteCutDirection tailCutDirection,
            SliderMidAnchorMode midAnchorMode,
            CustomData customData,
            Version version)
        {
            return new CustomSliderData(
                Type.Normal,
                colorType,
                false,
                headTime,
#if !PRE_V1_39_1
                headBeat,
                rotation,
#endif
                headLineIndex,
                headLineLayer,
                headBeforeJumpLineLayer,
                headControlPointLengthMultiplier,
                headCutDirection,
                0,
                false,
                tailTime,
#if !PRE_V1_39_1
                tailRotation,
#endif
                tailLineIndex,
                tailLineLayer,
                tailBeforeJumpLineLayer,
                tailControlPointLengthMultiplier,
                tailCutDirection,
                0,
                midAnchorMode,
                0,
                1,
                customData,
                version);
        }

        public static SliderData CreateCustomBurstSliderData(
            ColorType colorType,
            float headTime,
#if !PRE_V1_39_1
            float headBeat,
            int rotation,
#endif
            int headLineIndex,
            NoteLineLayer headLineLayer,
            NoteLineLayer headBeforeJumpLineLayer,
            NoteCutDirection headCutDirection,
            float tailTime,
#if !PRE_V1_39_1
            int tailRotation,
#endif
            int tailLineIndex,
            NoteLineLayer tailLineLayer,
            NoteLineLayer tailBeforeJumpLineLayer,
            int sliceCount,
            float squishAmount,
            CustomData customData,
            Version version)
        {
            return new CustomSliderData(
                Type.Burst,
                colorType,
                false,
                headTime,
#if !PRE_V1_39_1
                headBeat,
                rotation,
#endif
                headLineIndex,
                headLineLayer,
                headBeforeJumpLineLayer,
                0,
                headCutDirection,
                0,
                false,
                tailTime,
#if !PRE_V1_39_1
                tailRotation,
#endif
                tailLineIndex,
                tailLineLayer,
                tailBeforeJumpLineLayer,
                0,
                NoteCutDirection.Any,
                0,
                SliderMidAnchorMode.Straight,
                sliceCount,
                squishAmount,
                customData,
                version);
        }

        public override BeatmapDataItem GetCopy()
        {
            return new CustomSliderData(
                sliderType,
                colorType,
                hasHeadNote,
                time,
#if !PRE_V1_39_1
                beat,
                rotation,
#endif
                headLineIndex,
                headLineLayer,
                headBeforeJumpLineLayer,
                headControlPointLengthMultiplier,
                headCutDirection,
                headCutDirectionAngleOffset,
                hasTailNote,
                tailTime,
#if !PRE_V1_39_1
                tailRotation,
#endif
                tailLineIndex,
                tailLineLayer,
                tailBeforeJumpLineLayer,
                tailControlPointLengthMultiplier,
                tailCutDirection,
                tailCutDirectionAngleOffset,
                midAnchorMode,
                sliceCount,
                squishAmount,
                customData.Copy(),
                version);
        }
    }
}
