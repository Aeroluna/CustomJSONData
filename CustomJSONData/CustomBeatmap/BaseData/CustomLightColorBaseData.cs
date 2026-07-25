using System;

namespace CustomJSONData.CustomBeatmap.BaseData
{
    public class CustomLightColorBaseData : LightColorBaseData, ICustomData, IVersionable
    {
        public CustomLightColorBaseData(
            float beat,
#if PRE_V1_37_1
            BeatmapEventTransitionType transitionType,
#else
            bool usePreviousValue,
            EaseType easeType,
#endif
            EnvironmentColorType colorType,
            float brightness,
            int strobeBeatFrequency,
#if !V1_29_1
            float strobeBrightness,
            bool strobeFade,
#endif
            CustomData customData,
            Version version)
            : base(
                beat,
#if PRE_V1_37_1
                transitionType,
#else
                usePreviousValue,
                easeType,
#endif
                colorType,
                brightness,
#if !V1_29_1
                strobeBeatFrequency,
                strobeBrightness,
                strobeFade)
#else
                strobeBeatFrequency)
#endif
        {
            this.customData = customData;
            this.version = version;
        }

        public CustomData customData { get; }

        public Version version { get; }
    }
}
