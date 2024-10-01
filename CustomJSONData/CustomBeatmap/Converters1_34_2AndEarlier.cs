#if PRE_V1_37_1
using System;
using BeatmapSaveDataVersion3;
using HarmonyLib;
using JetBrains.Annotations;
using BpmProcessor = BeatmapDataLoader.BpmTimeProcessor;

namespace CustomJSONData.CustomBeatmap
{
    // TODO: event boxes
    public static class Converters
    {
        private static readonly Func<int, NoteLineLayer> _convertNoteLineLayer =
            (Func<int, NoteLineLayer>)Delegate.CreateDelegate(
                typeof(Func<int, NoteLineLayer>),
                typeof(BeatmapDataLoader),
                "ConvertNoteLineLayer",
                false,
                true)!;

        private static readonly Func<BeatmapSaveData.NoteColorType, ColorType> _convertColorType =
            (Func<BeatmapSaveData.NoteColorType, ColorType>)Delegate.CreateDelegate(
                typeof(Func<BeatmapSaveData.NoteColorType, ColorType>),
                typeof(BeatmapDataLoader),
                "ConvertColorType",
                false,
                true)!;

        public static CustomData GetData(this BeatmapSaveData.BeatmapSaveDataItem dataItem)
        {
            return dataItem is ICustomData customData
                ? customData.customData : new CustomData();
        }

        // rip virtualizer
        [HarmonyPatch(typeof(DataConvertor<BeatmapEventData>))]
        public static class CustomDataConverterPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch(nameof(DataConvertor<BeatmapEventData>.ProcessItem))]
            private static bool OverrideData(object __instance, object item, ref object __result)
            {
                switch (__instance)
                {
                    case CustomDataConverter<BeatmapObjectData> customObjectDataConverter:
                        __result = customObjectDataConverter.ProcessItem(item);
                        return false;
                    case CustomDataConverter<BeatmapEventData> customEventDataConverter:
                        __result = customEventDataConverter.ProcessItem(item);
                        return false;
                    default:
                        return true;
                }
            }
        }

        [UsedImplicitly]
        public class CustomDataConverter<T> : DataConvertor<T>
        {
            public new T ProcessItem(object item)
            {
                return _convertors.TryGetValue(CustomBeatmapData.GetCustomType(item), out DataItemConvertor<T> dataItemConvertor)
                    ? dataItemConvertor.Convert(item)
                    : default!;
            }
        }

        public abstract class VersionableBeatmapDataItemConverter<TBase, TIn, TOut>
            : BeatmapDataLoader.BeatmapDataItemConvertor<TBase, TIn, TOut>
            where TBase : BeatmapDataItem
            where TIn : BeatmapSaveData.BeatmapSaveDataItem
            where TOut : TBase
        {
            protected VersionableBeatmapDataItemConverter(BpmProcessor bpmTimeProcessor, BeatmapSaveData beatmapSaveData)
                : base(bpmTimeProcessor)
            {
                Version = beatmapSaveData is Version3CustomBeatmapSaveData customBeatmapSaveData
                    ? customBeatmapSaveData.beatmapVersion
                    : VersionExtensions.version3;
            }

            protected Version Version { get; }
        }

        public class CustomColorNoteConverter
            : VersionableBeatmapDataItemConverter<BeatmapObjectData, BeatmapSaveData.ColorNoteData, NoteData>
        {
            [UsedImplicitly]
            public CustomColorNoteConverter(BpmProcessor bpmTimeProcessor, BeatmapSaveData beatmapSaveData)
                : base(bpmTimeProcessor, beatmapSaveData)
            {
            }

            public override NoteData Convert(BeatmapSaveData.ColorNoteData data)
            {
                NoteData noteData = CustomNoteData.CreateCustomBasicNoteData(
                    BeatToTime(data.beat),
                    data.line,
                    _convertNoteLineLayer(data.layer),
                    _convertColorType(data.color),
                    data.cutDirection,
                    data.GetData(),
                    Version);
                noteData.SetCutDirectionAngleOffset(data.angleOffset);
                return noteData;
            }
        }

        public class CustomBombNoteConverter
            : VersionableBeatmapDataItemConverter<BeatmapObjectData, BeatmapSaveData.BombNoteData, NoteData>
        {
            [UsedImplicitly]
            public CustomBombNoteConverter(BpmProcessor bpmTimeProcessor, BeatmapSaveData beatmapSaveData)
                : base(bpmTimeProcessor, beatmapSaveData)
            {
            }

            public override NoteData Convert(BeatmapSaveData.BombNoteData data)
            {
                return CustomNoteData.CreateCustomBombNoteData(
                    BeatToTime(data.beat),
                    data.line,
                    _convertNoteLineLayer(data.layer),
                    data.GetData(),
                    Version);
            }
        }

        public class CustomObstacleConverter
            : VersionableBeatmapDataItemConverter<BeatmapObjectData, BeatmapSaveData.ObstacleData, ObstacleData>
        {
            [UsedImplicitly]
            public CustomObstacleConverter(BpmProcessor bpmTimeProcessor, BeatmapSaveData beatmapSaveData)
                : base(bpmTimeProcessor, beatmapSaveData)
            {
            }

            public override ObstacleData Convert(BeatmapSaveData.ObstacleData data)
            {
                float beat = BeatToTime(data.beat);
                return new CustomObstacleData(
                    beat,
                    data.line,
                    BeatmapDataLoader.ObstacleConvertor.GetNoteLineLayer(data.layer),
                    BeatToTime(data.beat + data.duration) - beat,
                    data.width,
                    data.height,
                    data.GetData(),
                    Version);
            }
        }

        public class CustomSliderConverter
            : VersionableBeatmapDataItemConverter<BeatmapObjectData, BeatmapSaveData.SliderData, SliderData>
        {
            [UsedImplicitly]
            public CustomSliderConverter(BpmProcessor bpmTimeProcessor, BeatmapSaveData beatmapSaveData)
                : base(bpmTimeProcessor, beatmapSaveData)
            {
            }

            public override SliderData Convert(BeatmapSaveData.SliderData data)
            {
                return CustomSliderData.CreateCustomSliderData(
                    _convertColorType(data.colorType),
                    BeatToTime(data.beat),
                    data.headLine,
                    _convertNoteLineLayer(data.headLayer),
                    _convertNoteLineLayer(data.headLayer),
                    data.headControlPointLengthMultiplier,
                    data.headCutDirection,
                    BeatToTime(data.tailBeat),
                    data.tailLine,
                    _convertNoteLineLayer(data.tailLayer),
                    _convertNoteLineLayer(data.tailLayer),
                    data.tailControlPointLengthMultiplier,
                    data.tailCutDirection,
                    data.sliderMidAnchorMode,
                    data.GetData(),
                    Version);
            }
        }

        public class CustomBurstSliderConverter
            : VersionableBeatmapDataItemConverter<BeatmapObjectData, BeatmapSaveData.BurstSliderData, SliderData>
        {
            [UsedImplicitly]
            public CustomBurstSliderConverter(BpmProcessor bpmTimeProcessor, BeatmapSaveData beatmapSaveData)
                : base(bpmTimeProcessor, beatmapSaveData)
            {
            }

            public override SliderData Convert(BeatmapSaveData.BurstSliderData data)
            {
                return CustomSliderData.CreateCustomBurstSliderData(
                    _convertColorType(data.colorType),
                    BeatToTime(data.beat),
                    data.headLine,
                    _convertNoteLineLayer(data.headLayer),
                    _convertNoteLineLayer(data.headLayer),
                    data.headCutDirection,
                    BeatToTime(data.tailBeat),
                    data.tailLine,
                    _convertNoteLineLayer(data.tailLayer),
                    _convertNoteLineLayer(data.tailLayer),
                    data.sliceCount,
                    data.squishAmount,
                    data.GetData(),
                    Version);
            }
        }

        public class CustomWaypointConverter
            : VersionableBeatmapDataItemConverter<BeatmapObjectData, BeatmapSaveData.WaypointData, WaypointData>
        {
            [UsedImplicitly]
            public CustomWaypointConverter(BpmProcessor bpmTimeProcessor, BeatmapSaveData beatmapSaveData)
                : base(bpmTimeProcessor, beatmapSaveData)
            {
            }

            public override WaypointData Convert(BeatmapSaveData.WaypointData data)
            {
                return new CustomWaypointData(
                    BeatToTime(data.beat),
                    data.line,
                    _convertNoteLineLayer(data.layer),
                    data.offsetDirection,
                    data.GetData(),
                    Version);
            }
        }

        public class CustomBpmEventConverter
            : VersionableBeatmapDataItemConverter<BeatmapEventData, BeatmapSaveData.BpmChangeEventData, BPMChangeBeatmapEventData>
        {
            [UsedImplicitly]
            public CustomBpmEventConverter(BpmProcessor bpmTimeProcessor, BeatmapSaveData beatmapSaveData)
                : base(bpmTimeProcessor, beatmapSaveData)
            {
            }

            public override BPMChangeBeatmapEventData Convert(BeatmapSaveData.BpmChangeEventData data)
            {
                return new CustomBPMChangeBeatmapEventData(
                    BeatToTime(data.beat),
                    data.bpm,
                    data.GetData(),
                    Version);
            }
        }

        public class CustomRotationEventConverter
            : VersionableBeatmapDataItemConverter<BeatmapEventData, BeatmapSaveData.RotationEventData, SpawnRotationBeatmapEventData>
        {
            [UsedImplicitly]
            public CustomRotationEventConverter(BpmProcessor bpmTimeProcessor, BeatmapSaveData beatmapSaveData)
                : base(bpmTimeProcessor, beatmapSaveData)
            {
            }

            public override SpawnRotationBeatmapEventData Convert(BeatmapSaveData.RotationEventData data)
            {
                SpawnRotationBeatmapEventData.SpawnRotationEventType executionTime =
                    data.executionTime == BeatmapSaveData.ExecutionTime.Early
                    ? SpawnRotationBeatmapEventData.SpawnRotationEventType.Early
                    : SpawnRotationBeatmapEventData.SpawnRotationEventType.Late;
                return new CustomSpawnRotationBeatmapEventdata(
                    BeatToTime(data.beat),
                    executionTime,
                    data.rotation,
                    data.GetData(),
                    Version);
            }
        }

        public class CustomBasicEventConverter
            : VersionableBeatmapDataItemConverter<BeatmapEventData, BeatmapSaveData.BasicEventData, BasicBeatmapEventData>
        {
            private readonly BeatmapDataLoader.SpecialEventsFilter _specialEventsFilter;

            [UsedImplicitly]
            public CustomBasicEventConverter(
                BpmProcessor bpmTimeProcessor,
                BeatmapDataLoader.SpecialEventsFilter specialEventsFilter,
                BeatmapSaveData beatmapSaveData)
                : base(bpmTimeProcessor, beatmapSaveData)
            {
                _specialEventsFilter = specialEventsFilter;
            }

            public override BasicBeatmapEventData Convert(BeatmapSaveData.BasicEventData data)
            {
                if (!_specialEventsFilter.IsEventValid(data.eventType))
                {
                    return null!;
                }

                return new CustomBasicBeatmapEventData(
                    BeatToTime(data.beat),
                    (BasicBeatmapEventType)data.eventType,
                    data.value,
                    data.floatValue,
                    data.GetData(),
                    Version);
            }
        }

        public class CustomColorBoostEventConverter
            : VersionableBeatmapDataItemConverter<BeatmapEventData, BeatmapSaveData.ColorBoostEventData, ColorBoostBeatmapEventData>
        {
            [UsedImplicitly]
            public CustomColorBoostEventConverter(BpmProcessor bpmTimeProcessor, BeatmapSaveData beatmapSaveData)
                : base(bpmTimeProcessor, beatmapSaveData)
            {
            }

            public override ColorBoostBeatmapEventData Convert(BeatmapSaveData.ColorBoostEventData data)
            {
                return new CustomColorBoostBeatmapEventData(
                    BeatToTime(data.beat),
                    data.boost,
                    data.GetData(),
                    Version);
            }
        }
    }
}
#endif
