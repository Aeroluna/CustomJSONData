using System;
using System.Collections.Generic;
using System.IO;
using BeatmapSaveDataVersion2_6_0AndEarlier;
using Newtonsoft.Json;

namespace CustomJSONData.CustomBeatmap
{
    public class Custom2_6_0AndEarlierBeatmapSaveData
    {
        private Custom2_6_0AndEarlierBeatmapSaveData(
            List<EventData> events,
            List<NoteData> notes,
            List<SliderData> sliders,
            List<WaypointData> waypoints,
            List<ObstacleData> obstacles,
            BeatmapSaveData.SpecialEventKeywordFiltersData specialEventsKeywordFilters,
            CustomData customData,
            List<CustomEventData> customEvents)
        {
            this.events = events;
            this.notes = notes;
            this.sliders = sliders;
            this.waypoints = waypoints;
            this.obstacles = obstacles;
            this.specialEventsKeywordFilters = specialEventsKeywordFilters;
            this.customData = customData;
            this.customEvents = customEvents;
        }

        public List<EventData> events { get; private set; }

        public List<NoteData> notes { get; }

        public List<SliderData> sliders { get; }

        public List<WaypointData> waypoints { get; }

        public List<ObstacleData> obstacles { get; }

        public BeatmapSaveData.SpecialEventKeywordFiltersData specialEventsKeywordFilters { get; }

        public List<CustomEventData> customEvents { get; }

        public CustomData customData { get; }

        public static Custom2_6_0AndEarlierBeatmapSaveData Deserialize(Version version, string path)
        {
            CustomData customData = new();
            List<CustomEventData> customEvents = new();
            List<EventData> events = new();
            List<NoteData> notes = new();
            List<SliderData> sliders = new();
            List<WaypointData> waypoints = new();
            List<ObstacleData> obstacles = new();
            List<BeatmapSaveData.SpecialEventsForKeyword> keywords = new();

            using JsonTextReader reader = new(new StreamReader(path));
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    switch (reader.Value)
                    {
                        default:
                            reader.Skip();
                            break;

                        case "_events":
                            DeserializeEventArray(reader, events);
                            break;

                        case "_notes":
                            DeserializeNoteArray(reader, notes);
                            break;

                        case "_sliders":
                            DeserializeSliderArray(reader, sliders);
                            break;

                        case "_waypoints":
                            DeserializeWaypointArray(reader, waypoints);
                            break;

                        case "_obstacles":
                            DeserializeObstacleArray(reader, obstacles);
                            break;

                        case "_specialEventsKeywordFilters":
                            reader.ReadObject(propertyName =>
                            {
                                if (propertyName.Equals("_keywords"))
                                {
                                    DeserializeKeywordArray(reader, keywords);
                                }
                                else
                                {
                                    reader.Skip();
                                }
                            });

                            break;

                        case "_customData":
                            reader.ReadToDictionary(customData, propertyName =>
                            {
                                if (!propertyName.Equals("_customEvents"))
                                {
                                    return true;
                                }

                                DeserializeCustomEventArray(reader, customEvents);
                                return false;
                            });

                            break;
                    }
                }
            }

            Custom2_6_0AndEarlierBeatmapSaveData beatmapSaveData = new(
                events,
                notes,
                sliders,
                waypoints,
                obstacles,
                new BeatmapSaveData.SpecialEventKeywordFiltersData(keywords),
                customData,
                customEvents);

            // wtf
            if (version.CompareTo(new Version("2.5.0")) < 0)
            {
                ConvertBeatmapSaveDataPreV2_5_0(beatmapSaveData);
            }

            return beatmapSaveData;
        }

        public static void DeserializeEventArray(JsonReader reader, List<EventData> list)
        {
            reader.ReadArray(() =>
            {
                float time = default;
                BeatmapSaveData.BeatmapEventType type = default;
                int value = default;
                float floatValue = default;
                CustomData data = new();
                return reader.ReadObject(objectName =>
                {
                    switch (objectName)
                    {
                        case "_time":
                            time = (float?)reader.ReadAsDouble() ?? time;
                            break;

                        case "_type":
                            type = (BeatmapSaveData.BeatmapEventType?)reader.ReadAsInt32Safe() ?? type;
                            break;

                        case "_value":
                            value = reader.ReadAsInt32Safe() ?? value;
                            break;

                        case "_floatValue":
                            floatValue = (float?)reader.ReadAsDouble() ?? floatValue;
                            break;

                        case "_customData":
                            reader.ReadToDictionary(data);
                            break;

                        default:
                            reader.Skip();
                            break;
                    }
                }).Finish(() => list.Add(new EventData(time, type, value, floatValue, data)));
            });
        }

        public static void DeserializeNoteArray(JsonReader reader, List<NoteData> list)
        {
            reader.ReadArray(() =>
            {
                float time = default;
                int lineIndex = default;
                NoteLineLayer lineLayer = default;
                BeatmapSaveData.NoteType type = default;
                NoteCutDirection cutDirection = default;
                CustomData data = new();
                return reader.ReadObject(objectName =>
                {
                    switch (objectName)
                    {
                        case "_time":
                            time = (float?)reader.ReadAsDouble() ?? time;
                            break;

                        case "_lineIndex":
                            lineIndex = reader.ReadAsInt32Safe() ?? lineIndex;
                            break;

                        case "_lineLayer":
                            lineLayer = (NoteLineLayer?)reader.ReadAsInt32Safe() ?? lineLayer;
                            break;

                        case "_type":
                            type = (BeatmapSaveData.NoteType?)reader.ReadAsInt32Safe() ?? type;
                            break;

                        case "_cutDirection":
                            cutDirection = (NoteCutDirection?)reader.ReadAsInt32Safe() ?? cutDirection;
                            break;

                        case "_customData":
                            reader.ReadToDictionary(data);
                            break;

                        default:
                            reader.Skip();
                            break;
                    }
                }).Finish(() => list.Add(new NoteData(time, lineIndex, lineLayer, type, cutDirection, data)));
            });
        }

        public static void DeserializeSliderArray(JsonReader reader, List<SliderData> list)
        {
            reader.ReadArray(() =>
            {
                float time = default;
                BeatmapSaveData.ColorType colorType = default;
                int headLineIndex = default;
                NoteLineLayer noteLineLayer = default;
                float headControlPointLengthMultiplier = default;
                NoteCutDirection noteCutDirection = default;
                float tailTime = default;
                int tailLineIndex = default;
                NoteLineLayer tailLineLayer = default;
                float tailControlPointLengthMultiplier = default;
                NoteCutDirection tailCutDirection = default;
                SliderMidAnchorMode sliderMidAnchorMode = default;
                CustomData data = new();
                return reader.ReadObject(objectName =>
                {
                    switch (objectName)
                    {
                        case "_time":
                            time = (float?)reader.ReadAsDouble() ?? time;
                            break;

                        case "_colorType":
                            colorType = (BeatmapSaveData.ColorType?)reader.ReadAsInt32Safe() ?? colorType;
                            break;

                        case "_headLineIndex":
                            headLineIndex = reader.ReadAsInt32Safe() ?? headLineIndex;
                            break;

                        case "_noteLineLayer":
                            noteLineLayer = (NoteLineLayer?)reader.ReadAsInt32Safe() ?? noteLineLayer;
                            break;

                        case "_headControlPointLengthMultiplier":
                            headControlPointLengthMultiplier =
                                (float?)reader.ReadAsDouble() ?? headControlPointLengthMultiplier;
                            break;

                        case "_noteCutDirection":
                            noteCutDirection = (NoteCutDirection?)reader.ReadAsInt32Safe() ?? noteCutDirection;
                            break;

                        case "_tailTime":
                            tailTime = (float?)reader.ReadAsDouble() ?? tailTime;
                            break;

                        case "_tailLineIndex":
                            tailLineIndex = reader.ReadAsInt32Safe() ?? tailLineIndex;
                            break;

                        case "_tailLineLayer":
                            tailLineLayer = (NoteLineLayer?)reader.ReadAsInt32Safe() ?? tailLineLayer;
                            break;

                        case "_tailControlPointLengthMultiplier":
                            tailControlPointLengthMultiplier =
                                (float?)reader.ReadAsDouble() ?? tailControlPointLengthMultiplier;
                            break;

                        case "_tailCutDirection":
                            tailCutDirection = (NoteCutDirection?)reader.ReadAsInt32Safe() ?? tailCutDirection;
                            break;

                        case "_sliderMidAnchorMode":
                            sliderMidAnchorMode = (SliderMidAnchorMode?)reader.ReadAsInt32Safe() ?? sliderMidAnchorMode;
                            break;

                        case "_customData":
                            reader.ReadToDictionary(data);
                            break;

                        default:
                            reader.Skip();
                            break;
                    }
                }).Finish(() => list.Add(new SliderData(
                    colorType,
                    time,
                    headLineIndex,
                    noteLineLayer,
                    headControlPointLengthMultiplier,
                    noteCutDirection,
                    tailTime,
                    tailLineIndex,
                    tailLineLayer,
                    tailControlPointLengthMultiplier,
                    tailCutDirection,
                    sliderMidAnchorMode,
                    data)));
            });
        }

        public static void DeserializeWaypointArray(JsonReader reader, List<WaypointData> list)
        {
            reader.ReadArray(() =>
            {
                float time = default;
                int lineIndex = default;
                NoteLineLayer lineLayer = default;
                OffsetDirection offsetDirection = default;
                CustomData data = new();
                return reader.ReadObject(objectName =>
                {
                    switch (objectName)
                    {
                        case "_time":
                            time = (float?)reader.ReadAsDouble() ?? time;
                            break;

                        case "_lineIndex":
                            lineIndex = reader.ReadAsInt32Safe() ?? lineIndex;
                            break;

                        case "_lineLayer":
                            lineLayer = (NoteLineLayer?)reader.ReadAsInt32Safe() ?? lineLayer;
                            break;

                        case "_offsetDirection":
                            offsetDirection = (OffsetDirection?)reader.ReadAsInt32Safe() ?? offsetDirection;
                            break;

                        case "_customData":
                            reader.ReadToDictionary(data);
                            break;

                        default:
                            reader.Skip();
                            break;
                    }
                }).Finish(() => list.Add(new WaypointData(time, lineIndex, lineLayer, offsetDirection, data)));
            });
        }

        public static void DeserializeObstacleArray(JsonReader reader, List<ObstacleData> list)
        {
            reader.ReadArray(() =>
            {
                float time = default;
                int lineIndex = default;
                BeatmapSaveData.ObstacleType type = default;
                float duration = default;
                int width = default;
                CustomData data = new();
                return reader.ReadObject(objectName =>
                {
                    switch (objectName)
                    {
                        case "_time":
                            time = (float?)reader.ReadAsDouble() ?? time;
                            break;

                        case "_lineIndex":
                            lineIndex = reader.ReadAsInt32Safe() ?? lineIndex;
                            break;

                        case "_type":
                            type = (BeatmapSaveData.ObstacleType?)reader.ReadAsInt32Safe() ?? type;
                            break;

                        case "_duration":
                            duration = (float?)reader.ReadAsDouble() ?? duration;
                            break;

                        case "_width":
                            width = reader.ReadAsInt32Safe() ?? width;
                            break;

                        case "_customData":
                            reader.ReadToDictionary(data);
                            break;

                        default:
                            reader.Skip();
                            break;
                    }
                }).Finish(() => list.Add(new ObstacleData(time, lineIndex, type, duration, width, data)));
            });
        }

        public static void DeserializeKeywordArray(JsonReader reader, List<BeatmapSaveData.SpecialEventsForKeyword> list)
        {
            reader.ReadArray(() =>
            {
                string keyword = string.Empty;
                List<BeatmapSaveData.BeatmapEventType> specialEvents = new();
                return reader.ReadObject(objectName =>
                {
                    switch (objectName)
                    {
                        case "_keyword":
                            keyword = reader.ReadAsString() ?? keyword;
                            break;

                        case "_specialEvents":
                            reader.Read();
                            if (reader.TokenType != JsonToken.StartArray)
                            {
                                throw new JsonSerializationException("_specialEvents was not array.");
                            }

                            while (true)
                            {
                                int? specialEvent = reader.ReadAsInt32Safe();
                                if (specialEvent.HasValue)
                                {
                                    specialEvents.Add((BeatmapSaveData.BeatmapEventType)specialEvent);
                                }
                                else
                                {
                                    if (reader.TokenType == JsonToken.EndArray)
                                    {
                                        break;
                                    }

                                    throw new JsonSerializationException("Value in _specialEvents was not int.");
                                }
                            }

                            break;

                        default:
                            reader.Skip();
                            break;
                    }
                }).Finish(() => list.Add(new BeatmapSaveData.SpecialEventsForKeyword(keyword, specialEvents)));
            });
        }

        public static void DeserializeCustomEventArray(JsonReader reader, List<CustomEventData> list)
        {
            reader.ReadArray(
                () =>
            {
                float time = default;
                string type = string.Empty;
                CustomData data = new();
                return reader.ReadObject(objectName =>
                {
                    switch (objectName)
                    {
                        case "_time":
                            time = (float?)reader.ReadAsDouble() ?? time;
                            break;

                        case "_type":
                            type = reader.ReadAsString() ?? type;
                            break;

                        case "_data":
                            reader.ReadToDictionary(data);
                            break;

                        default:
                            reader.Skip();
                            break;
                    }
                }).Finish(() => list.Add(new CustomEventData(time, type, data)));
            },
                false);
        }

        private static void ConvertBeatmapSaveDataPreV2_5_0(Custom2_6_0AndEarlierBeatmapSaveData beatmapSaveData)
        {
            List<EventData> list = new(beatmapSaveData.events.Count);
            foreach (EventData origEventData in beatmapSaveData.events)
            {
                EventData eventData = origEventData;
                if (eventData.type == BeatmapSaveData.BeatmapEventType.Event10)
                {
                    eventData = new EventData(eventData.time, BeatmapSaveData.BeatmapEventType.BpmChange, eventData.value, eventData.floatValue, eventData.customData);
                }

                if (eventData.type == BeatmapSaveData.BeatmapEventType.BpmChange)
                {
                    if (eventData.value != 0)
                    {
                        eventData = new EventData(eventData.time, eventData.type, eventData.value, eventData.value, eventData.customData);
                    }
                }
                else
                {
                    eventData = new EventData(eventData.time, eventData.type, eventData.value, 1f, eventData.customData);
                }

                list.Add(eventData);
            }

            beatmapSaveData.events = list;
        }

        public class EventData : BeatmapSaveData.EventData
        {
            internal EventData(float time, BeatmapSaveData.BeatmapEventType type, int value, float floatValue, CustomData customData)
                : base(time, type, value, floatValue)
            {
                this.customData = customData;
            }

            public CustomData customData { get; }
        }

        public class CustomEventData : BeatmapSaveDataItem
        {
            internal CustomEventData(float time, string type, CustomData data)
            {
                this.time = time;
                this.type = type;
                this.data = data;
            }

            public override float time { get; }

            public string type { get; }

            public CustomData data { get; }
        }

        public class NoteData : BeatmapSaveData.NoteData
        {
            internal NoteData(float time, int lineIndex, NoteLineLayer lineLayer, BeatmapSaveData.NoteType type, NoteCutDirection cutDirection, CustomData customData)
                : base(time, lineIndex, lineLayer, type, cutDirection)
            {
                this.customData = customData;
            }

            public CustomData customData { get; }
        }

        public class SliderData : BeatmapSaveData.SliderData
        {
            internal SliderData(
                BeatmapSaveData.ColorType colorType,
                float headTime,
                int headLineIndex,
                NoteLineLayer headLineLayer,
                float headControlPointLengthMultiplier,
                NoteCutDirection headCutDirection,
                float tailTime,
                int tailLineIndex,
                NoteLineLayer tailLineLayer,
                float tailControlPointLengthMultiplier,
                NoteCutDirection tailCutDirection,
                SliderMidAnchorMode sliderMidAnchorMode,
                CustomData customData)
                : base(
                    colorType,
                    headTime,
                    headLineIndex,
                    headLineLayer,
                    headControlPointLengthMultiplier,
                    headCutDirection,
                    tailTime,
                    tailLineIndex,
                    tailLineLayer,
                    tailControlPointLengthMultiplier,
                    tailCutDirection,
                    sliderMidAnchorMode)
            {
                this.customData = customData;
            }

            public CustomData customData { get; }
        }

        public class WaypointData : BeatmapSaveData.WaypointData
        {
            public WaypointData(float time, int lineIndex, NoteLineLayer lineLayer, OffsetDirection offsetDirection, CustomData customData)
                : base(time, lineIndex, lineLayer, offsetDirection)
            {
                this.customData = customData;
            }

            public CustomData customData { get; }
        }

        public class ObstacleData : BeatmapSaveData.ObstacleData
        {
            public ObstacleData(float time, int lineIndex, BeatmapSaveData.ObstacleType type, float duration, int width, CustomData customData)
                : base(time, lineIndex, type, duration, width)
            {
                this.customData = customData;
            }

            public CustomData customData { get; }
        }
    }
}
