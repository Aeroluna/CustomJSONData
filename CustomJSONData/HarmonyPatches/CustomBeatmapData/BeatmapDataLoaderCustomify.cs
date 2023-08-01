﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BeatmapSaveDataVersion3;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;

namespace CustomJSONData.HarmonyPatches
{
    [HarmonyPatch(typeof(BeatmapDataLoader))]
    internal static class BeatmapDataLoaderCustomify
    {
        private static readonly ConstructorInfo _beatmapDataCtor = AccessTools.FirstConstructor(typeof(BeatmapData), _ => true);
        private static readonly ConstructorInfo _bpmChangeCtor = AccessTools.FirstConstructor(typeof(BPMChangeBeatmapEventData), _ => true);
        private static readonly ConstructorInfo _bpmTimeProcessorCtor = AccessTools.FirstConstructor(typeof(BeatmapDataLoader.BpmTimeProcessor), _ => true);
        private static readonly MethodInfo _createCustomBeatmapData = AccessTools.Method(typeof(BeatmapDataLoaderCustomify), nameof(CreateCustomBeatmapData));
        private static readonly MethodInfo _createCustomBPMChangeData = AccessTools.Method(typeof(BeatmapDataLoaderCustomify), nameof(CreateCustomBPMChangeData));
        private static readonly MethodInfo _addCustomEvent = AccessTools.Method(typeof(BeatmapDataLoaderCustomify), nameof(AddCustomEvents));

        private static readonly MethodInfo _getBeatmapDataFromBeatmapSaveData = AccessTools.Method(typeof(BeatmapDataLoader), "GetBeatmapDataFromBeatmapSaveData");
        private static readonly MethodInfo _lockedMethod = AccessTools.Method(typeof(BeatmapDataLoaderCustomify), nameof(GetBeatmapDataLock));
        private static readonly object _lock = new();

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(BeatmapDataLoader.GetBeatmapDataFromSaveData))]
        private static IEnumerable<CodeInstruction> LockTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)

                // ReSharper disable once InconsistentlySynchronizedField
                .MatchForward(false, new CodeMatch(OpCodes.Call, _getBeatmapDataFromBeatmapSaveData))
                .SetOperandAndAdvance(_lockedMethod)
                .InstructionEnumeration();
        }

        // TODO: figure out what causes a race condition in the first place.
        private static BeatmapData GetBeatmapDataLock(
            BeatmapSaveData beatmapSaveData,
            BeatmapDifficulty beatmapDifficulty,
            float startBpm,
            bool loadingForDesignatedEnvironment,
            EnvironmentKeywords environmentKeywords,
            EnvironmentLightGroups environmentLightGroups,
            DefaultEnvironmentEvents defaultEnvironmentEvents,
            PlayerSpecificSettings playerSpecificSettings)
        {
            lock (_lock)
            {
                return (BeatmapData)_getBeatmapDataFromBeatmapSaveData.Invoke(
                    null,
                    new object[]
                    {
                        beatmapSaveData, beatmapDifficulty, startBpm, loadingForDesignatedEnvironment, environmentKeywords, environmentLightGroups,
                        defaultEnvironmentEvents, playerSpecificSettings
                    });
            }
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(BeatmapDataLoader.GetBeatmapDataFromBeatmapSaveData))]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Newobj, _beatmapDataCtor))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .Set(OpCodes.Call, _createCustomBeatmapData)

                .MatchForward(false, new CodeMatch(OpCodes.Newobj, _bpmChangeCtor))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .Set(OpCodes.Call, _createCustomBPMChangeData)

                .MatchForward(false, new CodeMatch(OpCodes.Newobj, _bpmTimeProcessorCtor))
                .Advance(2)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldloc_S, 4),
                    new CodeInstruction(OpCodes.Ldloc_2),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, _addCustomEvent))

                // "convertor" is NOT the correct spelling
                .ReplaceConverter<DataConvertor<BeatmapObjectData>, Converters.CustomDataConverter<BeatmapObjectData>>()

                .ReplaceVersionableConverter<BeatmapDataLoader.ColorNoteConvertor, Converters.CustomColorNoteConverter>()
                .ReplaceVersionableConverter<BeatmapDataLoader.BombNoteConvertor, Converters.CustomBombNoteConverter>()
                .ReplaceVersionableConverter<BeatmapDataLoader.ObstacleConvertor, Converters.CustomObstacleConverter>()
                .ReplaceVersionableConverter<BeatmapDataLoader.SliderConvertor, Converters.CustomSliderConverter>()
                .ReplaceConverter<BeatmapDataLoader.BurstSliderConvertor, Converters.CustomBurstSliderConverter>()
                .ReplaceVersionableConverter<BeatmapDataLoader.WaypointConvertor, Converters.CustomWaypointConverter>()

                .ReplaceVersionableConverter<DataConvertor<BeatmapEventData>, Converters.CustomDataConverter<BeatmapEventData>>()

                .ReplaceVersionableConverter<BeatmapDataLoader.BpmEventConvertor, Converters.CustomBpmEventConverter>()
                .ReplaceVersionableConverter<BeatmapDataLoader.RotationEventConvertor, Converters.CustomRotationEventConverter>()
                .ReplaceVersionableConverter<BeatmapDataLoader.BasicEventConvertor, Converters.CustomBasicEventConverter>()
                .ReplaceVersionableConverter<BeatmapDataLoader.ColorBoostEventConvertor, Converters.CustomColorBoostEventConverter>()

                // for reasons beyond my understanding, the leave will still take you to the InsertDefaultEnvironmentEvents, but inserting a nop fixes it...
                .End()
                .MatchBack(false, new CodeMatch(OpCodes.Leave))
                .Advance(1)
                .Insert(new CodeInstruction(OpCodes.Nop))

                .InstructionEnumeration();
        }

        private static CodeMatcher ReplaceVersionableConverter<TOriginal, TCustom>(this CodeMatcher matcher)
        {
            ConstructorInfo original = AccessTools.FirstConstructor(typeof(TOriginal), _ => true);
            ConstructorInfo custom = AccessTools.FirstConstructor(typeof(TCustom), _ => true);

            return matcher.MatchForward(false, new CodeMatch(OpCodes.Newobj, original))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .SetOperandAndAdvance(custom);
        }

        private static CodeMatcher ReplaceConverter<TOriginal, TCustom>(this CodeMatcher matcher)
        {
            ConstructorInfo original = AccessTools.FirstConstructor(typeof(TOriginal), _ => true);
            ConstructorInfo custom = AccessTools.FirstConstructor(typeof(TCustom), _ => true);

            return matcher.MatchForward(false, new CodeMatch(OpCodes.Newobj, original))
                .SetOperandAndAdvance(custom);
        }

        private static BeatmapData CreateCustomBeatmapData(int numberOfLines, BeatmapSaveData beatmapSaveData)
        {
            if (beatmapSaveData is CustomBeatmapSaveData customBeatmapSaveData)
            {
                return new CustomBeatmapData(
                    numberOfLines,
                    customBeatmapSaveData.version2_6_0AndEarlier,
                    customBeatmapSaveData.customData,
                    customBeatmapSaveData.beatmapCustomData,
                    customBeatmapSaveData.levelCustomData);
            }

            return new CustomBeatmapData(
                4,
                false,
                new CustomData(),
                new CustomData(),
                new CustomData());
        }

        private static BPMChangeBeatmapEventData CreateCustomBPMChangeData(float time, float bpm, BeatmapSaveData beatmapSaveData)
        {
            return new CustomBPMChangeBeatmapEventData(
                time,
                bpm,
                new CustomData(),
                beatmapSaveData is CustomBeatmapSaveData { version2_6_0AndEarlier: true });
        }

        private static void AddCustomEvents(
            BeatmapDataLoader.BpmTimeProcessor timeProcessor,
            CustomBeatmapData beatmapData,
            BeatmapSaveData saveData)
        {
            if (saveData is not CustomBeatmapSaveData customSaveData)
            {
                return;
            }

            foreach (CustomBeatmapSaveData.CustomEventData customEventSaveData in customSaveData.customEvents.OrderBy(n => n))
            {
                beatmapData.InsertCustomEventData(new CustomEventData(
                    timeProcessor.ConvertBeatToTime(customEventSaveData.beat),
                    customEventSaveData.type,
                    customEventSaveData.customData,
                    customSaveData.version2_6_0AndEarlier));
            }
        }
    }
}
