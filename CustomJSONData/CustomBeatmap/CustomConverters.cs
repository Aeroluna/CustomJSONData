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

        public class CustomColorNoteConverter
            : BeatmapDataLoader.BeatmapDataItemConvertor<BeatmapObjectData, BeatmapSaveData.ColorNoteData, NoteData>
        {
            [UsedImplicitly]
            public CustomColorNoteConverter(BpmProcessor bpmTimeProcessor)
                : base(bpmTimeProcessor)
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
                    data.GetData());
                noteData.SetCutDirectionAngleOffset(data.angleOffset);
                return noteData;
            }
        }

        public class CustomBombNoteConverter
            : BeatmapDataLoader.BeatmapDataItemConvertor<BeatmapObjectData, BeatmapSaveData.BombNoteData, NoteData>
        {
            [UsedImplicitly]
            public CustomBombNoteConverter(BpmProcessor bpmTimeProcessor)
                : base(bpmTimeProcessor)
            {
            }

            public override NoteData Convert(BeatmapSaveData.BombNoteData data)
            {
                return CustomNoteData.CreateCustomBombNoteData(
                    BeatToTime(data.beat),
                    data.line,
                    _convertNoteLineLayer(data.layer),
                    data.GetData());
            }
        }

        public class CustomObstacleConverter
            : BeatmapDataLoader.BeatmapDataItemConvertor<BeatmapObjectData, BeatmapSaveData.ObstacleData, ObstacleData>
        {
            private static readonly Func<int, NoteLineLayer> _getNoteLineLayer =
                (Func<int, NoteLineLayer>)Delegate.CreateDelegate(
                    typeof(Func<int, NoteLineLayer>),
                    typeof(BeatmapDataLoader.ObstacleConvertor),
                    "GetNoteLineLayer",
                    false,
                    true)!;

            [UsedImplicitly]
            public CustomObstacleConverter(BpmProcessor bpmTimeProcessor)
                : base(bpmTimeProcessor)
            {
            }

            public override ObstacleData Convert(BeatmapSaveData.ObstacleData data)
            {
                float beat = BeatToTime(data.beat);
                return new CustomObstacleData(
                    beat,
                    data.line,
                    _getNoteLineLayer(data.layer),
                    BeatToTime(data.beat + data.duration) - beat,
                    data.width,
                    data.height,
                    data.GetData());
            }
        }

        public class CustomSliderConverter
            : BeatmapDataLoader.BeatmapDataItemConvertor<BeatmapObjectData, BeatmapSaveData.SliderData, SliderData>
        {
            [UsedImplicitly]
            public CustomSliderConverter(BpmProcessor bpmTimeProcessor)
                : base(bpmTimeProcessor)
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
                    data.GetData());
            }
        }

        public class CustomBurstSliderConverter
            : BeatmapDataLoader.BeatmapDataItemConvertor<BeatmapObjectData, BeatmapSaveData.BurstSliderData, SliderData>
        {
            [UsedImplicitly]
            public CustomBurstSliderConverter(BpmProcessor bpmTimeProcessor)
                : base(bpmTimeProcessor)
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
                    NoteCutDirection.Any,
                    data.sliceCount,
                    data.squishAmount,
                    data.GetData());
            }
        }

        public class CustomWaypointConverter
            : BeatmapDataLoader.BeatmapDataItemConvertor<BeatmapObjectData, BeatmapSaveData.WaypointData, WaypointData>
        {
            [UsedImplicitly]
            public CustomWaypointConverter(BpmProcessor bpmTimeProcessor)
                : base(bpmTimeProcessor)
            {
            }

            public override WaypointData Convert(BeatmapSaveData.WaypointData data)
            {
                return new CustomWaypointData(
                    BeatToTime(data.beat),
                    data.line,
                    _convertNoteLineLayer(data.layer),
                    data.offsetDirection,
                    data.GetData());
            }
        }

        public class CustomBpmEventConverter
            : BeatmapDataLoader.BeatmapDataItemConvertor<BeatmapEventData, BeatmapSaveData.BpmChangeEventData, BPMChangeBeatmapEventData>
        {
            [UsedImplicitly]
            public CustomBpmEventConverter(BpmProcessor bpmTimeProcessor)
                : base(bpmTimeProcessor)
            {
            }

            public override BPMChangeBeatmapEventData Convert(BeatmapSaveData.BpmChangeEventData data)
            {
                return new CustomBPMChangeBeatmapEventData(
                    BeatToTime(data.beat),
                    data.bpm,
                    data.GetData());
            }
        }

        public class CustomRotationEventConverter
            : BeatmapDataLoader.BeatmapDataItemConvertor<BeatmapEventData, BeatmapSaveData.RotationEventData, SpawnRotationBeatmapEventData>
        {
            [UsedImplicitly]
            public CustomRotationEventConverter(BpmProcessor bpmTimeProcessor)
                : base(bpmTimeProcessor)
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
                    data.GetData());
            }
        }

        public class CustomBasicEventConverter
            : BeatmapDataLoader.BeatmapDataItemConvertor<BeatmapEventData, BeatmapSaveData.BasicEventData, BasicBeatmapEventData>
        {
            private readonly BeatmapDataLoader.SpecialEventsFilter _specialEventsFilter;

            [UsedImplicitly]
            public CustomBasicEventConverter(
                BpmProcessor bpmTimeProcessor,
                BeatmapDataLoader.SpecialEventsFilter specialEventsFilter)
                : base(bpmTimeProcessor)
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
                    data.GetData());
            }
        }

        public class CustomColorBoostEventConverter
            : BeatmapDataLoader.BeatmapDataItemConvertor<BeatmapEventData, BeatmapSaveData.ColorBoostEventData, ColorBoostBeatmapEventData>
        {
            [UsedImplicitly]
            public CustomColorBoostEventConverter(BpmProcessor bpmTimeProcessor)
                : base(bpmTimeProcessor)
            {
            }

            public override ColorBoostBeatmapEventData Convert(BeatmapSaveData.ColorBoostEventData data)
            {
                return new CustomColorBoostBeatmapEventData(
                    BeatToTime(data.beat),
                    data.boost,
                    data.GetData());
            }
        }
    }
}
