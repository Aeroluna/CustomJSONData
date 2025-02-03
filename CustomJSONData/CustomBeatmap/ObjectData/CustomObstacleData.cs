using System;

namespace CustomJSONData.CustomBeatmap
{
    public class CustomObstacleData : ObstacleData, ICustomData, IVersionable
    {
        public CustomObstacleData(
            float time,
#if !PRE_V1_39_1
            float startBeat,
            float endBeat,
            int rotation,
#endif
            int lineIndex,
            NoteLineLayer lineLayer,
            float duration,
            int width,
            int height,
            CustomData customData,
            Version version)
            : base(
                time,
#if !PRE_V1_39_1
                startBeat,
                endBeat,
                rotation,
#endif
                lineIndex,
                lineLayer,
                duration,
                width,
                height)
        {
            this.customData = customData;
            this.version = version;
        }

        public CustomData customData { get; }

        public Version version { get; }

        public override BeatmapDataItem GetCopy()
        {
            return new CustomObstacleData(
                time,
#if !PRE_V1_39_1
                beat,
                endBeat,
                rotation,
#endif
                lineIndex,
                lineLayer,
                duration,
                width,
                height,
                customData.Copy(),
                version);
        }
    }
}
