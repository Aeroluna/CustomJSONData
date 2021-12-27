using System;
using System.Collections.Generic;
using CustomJSONData.CustomBeatmap;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomJSONData
{
    public class CustomEventCallbackController : MonoBehaviour
    {
        private static readonly FieldAccessor<BeatmapObjectCallbackController, IAudioTimeSource>.Accessor _audioTimeSourceAccessor = FieldAccessor<BeatmapObjectCallbackController, IAudioTimeSource>.GetAccessor("_audioTimeSource");
        private static readonly FieldAccessor<BeatmapObjectCallbackController, float>.Accessor _spawningStartTimeAccessor = FieldAccessor<BeatmapObjectCallbackController, float>.GetAccessor("_spawningStartTime");

        private readonly List<CustomEventCallbackData> _customEventCallbackData = new();

        private BeatmapObjectCallbackController? _beatmapObjectCallbackController;

        public delegate void CustomEventCallback(CustomEventData eventData);

        [PublicAPI]
        public static event Action<CustomEventCallbackController>? didInitEvent;

        public CustomBeatmapData? BeatmapData { get; private set; }

        [PublicAPI]
        public BeatmapObjectCallbackController? BeatmapObjectCallbackController => _beatmapObjectCallbackController;

        public IAudioTimeSource? AudioTimeSource => _audioTimeSourceAccessor(ref _beatmapObjectCallbackController!);

        public float? SpawningStartTime => _spawningStartTimeAccessor(ref _beatmapObjectCallbackController!);

        [PublicAPI]
        public CustomEventCallbackData AddCustomEventCallback(CustomEventCallback callback, float aheadTime = 0, bool callIfBeforeStartTime = true)
        {
            CustomEventCallbackData customEventCallbackData = new(callback, aheadTime, callIfBeforeStartTime);
            _customEventCallbackData.Add(customEventCallbackData);
            return customEventCallbackData;
        }

        public void RemoveBeatmapEventCallback(CustomEventCallbackData callbackData)
        {
            _customEventCallbackData.Remove(callbackData);
        }

        public void InvokeCustomEvent(CustomEventData customEventData)
        {
            foreach (CustomEventCallbackData t in _customEventCallbackData)
            {
                t.callback(customEventData);
            }
        }

        internal void SetNewBeatmapData(IReadonlyBeatmapData beatmapData)
        {
            BeatmapData = (CustomBeatmapData)beatmapData;

            foreach (CustomEventCallbackData customEventCallbackData in _customEventCallbackData)
            {
                customEventCallbackData.nextEventIndex = 0;
            }
        }

        internal void Init(BeatmapObjectCallbackController beatmapObjectCallbackController, IReadonlyBeatmapData beatmapData)
        {
            _beatmapObjectCallbackController = beatmapObjectCallbackController;
            SetNewBeatmapData(beatmapData);
            didInitEvent?.Invoke(this);
        }

        private void LateUpdate()
        {
            if (!_beatmapObjectCallbackController!.enabled || BeatmapData == null)
            {
                return;
            }

            bool start = false;

            foreach (CustomEventCallbackData customEventCallbackData in _customEventCallbackData)
            {
                while (customEventCallbackData.nextEventIndex < BeatmapData.customEventsData.Count)
                {
                    CustomEventData customEventData = BeatmapData.customEventsData[customEventCallbackData.nextEventIndex];
                    if (customEventData.time - customEventCallbackData.aheadTime >= AudioTimeSource?.songTime)
                    {
                        break;
                    }

                    // Events are before/during start
                    // This should only be done if the starting time is 0
                    if (customEventData.time <= SpawningStartTime && SpawningStartTime == 0)
                    {
                        start = true;
                        // TODO: Pause the song so the audio doesn't play while loading?
                    }

                    // skip events before song start
                    if (customEventData.time >= SpawningStartTime || customEventCallbackData.callIfBeforeStartTime)
                    {
                        customEventCallbackData.callback(customEventData);
                    }

                    customEventCallbackData.nextEventIndex++;
                }
            }

            // Restart the song since we were loading for events
            // this *should* fix the issues caused by the map running in the background while events were loading
            if (!start || AudioTimeSource is not AudioTimeSyncController implAudioTimeSource)
            {
                return;
            }

            implAudioTimeSource.StartSong(0);
            implAudioTimeSource.Resume();
        }

        public class CustomEventCallbackData
        {
            public CustomEventCallbackData(CustomEventCallback callback, float aheadTime, bool callIfBeforeStartTime)
            {
                this.callback = callback;
                this.aheadTime = aheadTime;
                this.callIfBeforeStartTime = callIfBeforeStartTime;
                nextEventIndex = 0;
            }

            public CustomEventCallback callback { get; }

            public float aheadTime { get; }

            public int nextEventIndex { get; set; }

            public bool callIfBeforeStartTime { get; }
        }
    }
}
