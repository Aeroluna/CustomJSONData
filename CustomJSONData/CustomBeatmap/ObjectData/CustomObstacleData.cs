using System;

namespace CustomJSONData.CustomBeatmap
{
    public class CustomObstacleData : ObstacleData, ICustomData, IVersionable
    {
        public CustomObstacleData(float time, int lineIndex, NoteLineLayer lineLayer, float duration, int width, int height, CustomData customData, Version version)
            : base(time, lineIndex, lineLayer, duration, width, height)
        {
            this.customData = customData;
            this.version = version;
        }

        public CustomData customData { get; }

        public Version version { get; }

        public override BeatmapDataItem GetCopy()
        {
            return new CustomObstacleData(time, lineIndex, lineLayer, duration, width, height, customData.Copy(), version);
        }
    }
}
