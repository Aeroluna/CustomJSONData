using System.Collections.Generic;
#if !PRE_V1_37_1
using BeatmapSaveDataVersion3;
using _BasicEventData = BeatmapSaveDataVersion3.BasicEventData;
using _BeatmapEventType = BeatmapSaveDataCommon.BeatmapEventType;
using _BombNoteData = BeatmapSaveDataVersion3.BombNoteData;
using _BpmChangeEventData = BeatmapSaveDataVersion3.BpmChangeEventData;
using _BurstSliderData = BeatmapSaveDataVersion3.BurstSliderData;
using _ColorBoostEventData = BeatmapSaveDataVersion3.ColorBoostEventData;
using _ColorNoteData = BeatmapSaveDataVersion3.ColorNoteData;
using _ExecutionTime = BeatmapSaveDataCommon.ExecutionTime;
using _LightColorEventBoxGroup = BeatmapSaveDataVersion3.LightColorEventBoxGroup;
using _LightRotationEventBoxGroup = BeatmapSaveDataVersion3.LightRotationEventBoxGroup;
using _NoteColorType = BeatmapSaveDataCommon.NoteColorType;
using _NoteCutDirection = BeatmapSaveDataCommon.NoteCutDirection;
using _ObstacleData = BeatmapSaveDataVersion3.ObstacleData;
using _OffsetDirection = BeatmapSaveDataCommon.OffsetDirection;
using _RotationEventData = BeatmapSaveDataVersion3.RotationEventData;
using _SliderData = BeatmapSaveDataVersion3.SliderData;
using _SliderMidAnchorMode = BeatmapSaveDataCommon.SliderMidAnchorMode;
using _WaypointData = BeatmapSaveDataVersion3.WaypointData;
#else
using _BasicEventData = BeatmapSaveDataVersion3.BeatmapSaveData.BasicEventData;
using _BeatmapEventType = BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData.BeatmapEventType;
using _BombNoteData = BeatmapSaveDataVersion3.BeatmapSaveData.BombNoteData;
using _BpmChangeEventData = BeatmapSaveDataVersion3.BeatmapSaveData.BpmChangeEventData;
using _BurstSliderData = BeatmapSaveDataVersion3.BeatmapSaveData.BurstSliderData;
using _ColorBoostEventData = BeatmapSaveDataVersion3.BeatmapSaveData.ColorBoostEventData;
using _ColorNoteData = BeatmapSaveDataVersion3.BeatmapSaveData.ColorNoteData;
using _ExecutionTime = BeatmapSaveDataVersion3.BeatmapSaveData.ExecutionTime;
using _LightColorEventBoxGroup = BeatmapSaveDataVersion3.BeatmapSaveData.LightColorEventBoxGroup;
using _LightRotationEventBoxGroup = BeatmapSaveDataVersion3.BeatmapSaveData.LightRotationEventBoxGroup;
using _NoteColorType = BeatmapSaveDataVersion3.BeatmapSaveData.NoteColorType;
using _NoteCutDirection = NoteCutDirection;
using _ObstacleData = BeatmapSaveDataVersion3.BeatmapSaveData.ObstacleData;
using _OffsetDirection = OffsetDirection;
using _RotationEventData = BeatmapSaveDataVersion3.BeatmapSaveData.RotationEventData;
using _SliderData = BeatmapSaveDataVersion3.BeatmapSaveData.SliderData;
using _SliderMidAnchorMode = SliderMidAnchorMode;
using _WaypointData = BeatmapSaveDataVersion3.BeatmapSaveData.WaypointData;
#endif

namespace CustomJSONData.CustomBeatmap
{
    public partial class Version3CustomBeatmapSaveData
    {
        public class CustomEventSaveData : BeatmapSaveDataItem, ICustomData
        {
            internal CustomEventSaveData(float beat, string type, CustomData data)
                : base(beat)
            {
                this.type = type;
                customData = data;
            }

            public string type { get; }

            public CustomData customData { get; }
        }

        public class BasicEventSaveData : _BasicEventData, ICustomData
        {
            public BasicEventSaveData(
                float beat,
                _BeatmapEventType eventType,
                int value,
                float floatValue,
                CustomData customData)
                : base(beat, eventType, value, floatValue)
            {
                this.customData = customData;
            }

            public CustomData customData { get; }
        }

        public class ColorBoostEventSaveData : _ColorBoostEventData, ICustomData
        {
            public ColorBoostEventSaveData(
                float beat,
                bool boost,
                CustomData customData)
                : base(beat, boost)
            {
                this.customData = customData;
            }

            public CustomData customData { get; }
        }

        public class BpmChangeEventSaveData : _BpmChangeEventData, ICustomData
        {
            public BpmChangeEventSaveData(
                float beat,
                float bpm,
                CustomData customData)
                : base(beat, bpm)
            {
                this.customData = customData;
            }

            public CustomData customData { get; }
        }

        public class RotationEventSaveData : _RotationEventData, ICustomData
        {
            public RotationEventSaveData(
                float beat,
                _ExecutionTime executionTime,
                float rotation,
                CustomData customData)
                : base(beat, executionTime, rotation)
            {
                this.customData = customData;
            }

            public CustomData customData { get; }
        }

        public class LightColorEventBoxGroupSaveData : _LightColorEventBoxGroup, ICustomData
        {
            public LightColorEventBoxGroupSaveData(
                float beat,
                int groupId,
                List<LightColorEventBox> eventBoxes,
                CustomData customData)
                : base(beat, groupId, eventBoxes)
            {
                this.customData = customData;
            }

            public CustomData customData { get; }
        }

        public class LightRotationEventBoxGroupSaveData : _LightRotationEventBoxGroup, ICustomData
        {
            public LightRotationEventBoxGroupSaveData(
                float beat,
                int groupId,
                List<LightRotationEventBox> eventBoxes,
                CustomData customData)
                : base(beat, groupId, eventBoxes)
            {
                this.customData = customData;
            }

            public CustomData customData { get; }
        }

        public class ColorNoteSaveData : _ColorNoteData, ICustomData
        {
            public ColorNoteSaveData(
                float beat,
                int line,
                int layer,
                _NoteColorType color,
                _NoteCutDirection cutDirection,
                int angleOffset,
                CustomData customData)
                : base(beat, line, layer, color, cutDirection, angleOffset)
            {
                this.customData = customData;
            }

            public CustomData customData { get; }
        }

        public class BombNoteSaveData : _BombNoteData, ICustomData
        {
            public BombNoteSaveData(
                float beat,
                int line,
                int layer,
                CustomData customData)
                : base(beat, line, layer)
            {
                this.customData = customData;
            }

            public CustomData customData { get; }
        }

        public class WaypointSaveData : _WaypointData, ICustomData
        {
            public WaypointSaveData(
                float beat,
                int line,
                int layer,
                _OffsetDirection offsetDirection,
                CustomData customData)
                : base(beat, line, layer, offsetDirection)
            {
                this.customData = customData;
            }

            public CustomData customData { get; }
        }

        public class SliderSaveData : _SliderData, ICustomData
        {
            public SliderSaveData(
                _NoteColorType colorType,
                float headBeat,
                int headLine,
                int headLayer,
                float headControlPointLengthMultiplier,
                _NoteCutDirection headCutDirection,
                float tailBeat,
                int tailLine,
                int tailLayer,
                float tailControlPointLengthMultiplier,
                _NoteCutDirection tailCutDirection,
                _SliderMidAnchorMode sliderMidAnchorMode,
                CustomData customData)
                : base(
                    colorType,
                    headBeat,
                    headLine,
                    headLayer,
                    headControlPointLengthMultiplier,
                    headCutDirection,
                    tailBeat,
                    tailLine,
                    tailLayer,
                    tailControlPointLengthMultiplier,
                    tailCutDirection,
                    sliderMidAnchorMode)
            {
                this.customData = customData;
            }

            public CustomData customData { get; }
        }

        public class BurstSliderSaveData : _BurstSliderData, ICustomData
        {
            public BurstSliderSaveData(
                _NoteColorType colorType,
                float headBeat,
                int headLine,
                int headLayer,
                _NoteCutDirection headCutDirection,
                float tailBeat,
                int tailLine,
                int tailLayer,
                int sliceCount,
                float squishAmount,
                CustomData customData)
                : base(
                    colorType,
                    headBeat,
                    headLine,
                    headLayer,
                    headCutDirection,
                    tailBeat,
                    tailLine,
                    tailLayer,
                    sliceCount,
                    squishAmount)
            {
                this.customData = customData;
            }

            public CustomData customData { get; }
        }

        public class ObstacleSaveData : _ObstacleData, ICustomData
        {
            public ObstacleSaveData(
                float beat,
                int line,
                int layer,
                float duration,
                int width,
                int height,
                CustomData customData)
                : base(beat, line, layer, duration, width, height)
            {
                this.customData = customData;
            }

            public CustomData customData { get; }
        }
    }
}
