using System;

namespace CustomJSONData.CustomBeatmap
{
    public class CustomObstacleData : ObstacleData, ICustomData, IVersionable
    {
        public CustomObstacleData(
            float time,
#if LATEST
            float startBeat,
            float endBeat,
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
#if LATEST
                startBeat,
                endBeat,
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
#if LATEST
                beat,
                endBeat,
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
