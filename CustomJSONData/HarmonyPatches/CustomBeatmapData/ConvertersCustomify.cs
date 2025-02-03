#if !PRE_V1_37_1
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BeatmapSaveDataCommon;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;

namespace CustomJSONData.HarmonyPatches
{
    [HarmonyPatch]
    internal static class ConvertersCustomify
    {
        private static readonly MethodInfo _getData = AccessTools.Method(typeof(ConvertersCustomify), nameof(GetData));

        private static readonly MethodInfo _createBasicNoteData = AccessTools.Method(typeof(NoteData), nameof(NoteData.CreateBasicNoteData));
        private static readonly MethodInfo _createCustomBasicNoteData = AccessTools.Method(typeof(CustomNoteData), nameof(CustomNoteData.CreateCustomBasicNoteData));

        private static readonly MethodInfo _createBombNoteData = AccessTools.Method(typeof(NoteData), nameof(NoteData.CreateBombNoteData));
        private static readonly MethodInfo _createCustomBombNoteData = AccessTools.Method(typeof(CustomNoteData), nameof(CustomNoteData.CreateCustomBombNoteData));

        private static readonly ConstructorInfo _obstacleDataCtor = AccessTools.FirstConstructor(typeof(ObstacleData), _ => true);
        private static readonly ConstructorInfo _customObstacleDataCtor = AccessTools.FirstConstructor(typeof(CustomObstacleData), _ => true);

        private static readonly MethodInfo _createSliderData = AccessTools.Method(typeof(SliderData), nameof(SliderData.CreateSliderData));
        private static readonly MethodInfo _createCustomSliderData = AccessTools.Method(typeof(CustomSliderData), nameof(CustomSliderData.CreateCustomSliderData));

        private static readonly MethodInfo _createBurstSliderData = AccessTools.Method(typeof(SliderData), nameof(SliderData.CreateBurstSliderData));
        private static readonly MethodInfo _createCustomBurstSliderData = AccessTools.Method(typeof(CustomSliderData), nameof(CustomSliderData.CreateCustomBurstSliderData));

        private static readonly ConstructorInfo _waypointCtor = AccessTools.FirstConstructor(typeof(WaypointData), _ => true);
        private static readonly ConstructorInfo _customWaypointCtor = AccessTools.FirstConstructor(typeof(CustomWaypointData), _ => true);

        private static readonly ConstructorInfo _bpmEventCtor = AccessTools.FirstConstructor(typeof(BPMChangeBeatmapEventData), _ => true);
        private static readonly ConstructorInfo _customBpmEventCtor = AccessTools.FirstConstructor(typeof(CustomBPMChangeBeatmapEventData), _ => true);

#if PRE_V1_39_1
        private static readonly ConstructorInfo _rotationEventCtor = AccessTools.FirstConstructor(typeof(SpawnRotationBeatmapEventData), _ => true);
        private static readonly ConstructorInfo _customRotationEventCtor = AccessTools.FirstConstructor(typeof(CustomSpawnRotationBeatmapEventdata), _ => true);
#endif

        private static readonly ConstructorInfo _basicEventCtor = AccessTools.FirstConstructor(typeof(BasicBeatmapEventData), _ => true);
        private static readonly ConstructorInfo _customBasicEventCtor = AccessTools.FirstConstructor(typeof(CustomBasicBeatmapEventData), _ => true);

        private static readonly ConstructorInfo _colorBoostEventCtor = AccessTools.FirstConstructor(typeof(ColorBoostBeatmapEventData), _ => true);
        private static readonly ConstructorInfo _customColorBoostEventCtor = AccessTools.FirstConstructor(typeof(CustomColorBoostBeatmapEventData), _ => true);

        private static readonly FieldInfo _version2 = AccessTools.Field(typeof(BeatmapSaveDataHelpers), nameof(BeatmapSaveDataHelpers.version2));
        private static readonly FieldInfo _version3 = AccessTools.Field(typeof(BeatmapSaveDataHelpers), nameof(BeatmapSaveDataHelpers.version3));

        private static CustomData GetData(this IBeat dataItem)
        {
            return dataItem is ICustomData customData
                ? customData.customData : new CustomData();
        }

        private static IEnumerable<CodeInstruction> ReplaceMethod(
            this IEnumerable<CodeInstruction> instructions,
            FieldInfo field,
            MethodInfo original,
            MethodInfo replace)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Call, original))
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Call, _getData),
                    new CodeInstruction(OpCodes.Ldsfld, field))
                .SetOperandAndAdvance(replace)
                .InstructionEnumeration();
        }

        private static IEnumerable<CodeInstruction> ReplaceCtor(
            this IEnumerable<CodeInstruction> instructions,
            FieldInfo field,
            ConstructorInfo original,
            ConstructorInfo replace)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Newobj, original))
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Call, _getData),
                    new CodeInstruction(OpCodes.Ldsfld, field))
                .SetOperandAndAdvance(replace)
                .InstructionEnumeration();
        }

        // VERSION 3
        [HarmonyTranspiler]
        [HarmonyPatch(
            typeof(BeatmapDataLoaderVersion3.BeatmapDataLoader.ColorNoteConverter),
            nameof(BeatmapDataLoaderVersion3.BeatmapDataLoader.ColorNoteConverter.Convert))]
        private static IEnumerable<CodeInstruction> ColorNoteConvertV3(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.ReplaceMethod(_version3, _createBasicNoteData, _createCustomBasicNoteData);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(
            typeof(BeatmapDataLoaderVersion3.BeatmapDataLoader.BombNoteConverter),
            nameof(BeatmapDataLoaderVersion3.BeatmapDataLoader.BombNoteConverter.Convert))]
        private static IEnumerable<CodeInstruction> BombNoteConvertV3(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.ReplaceMethod(_version3, _createBombNoteData, _createCustomBombNoteData);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(
            typeof(BeatmapDataLoaderVersion3.BeatmapDataLoader.ObstacleConverter),
            nameof(BeatmapDataLoaderVersion3.BeatmapDataLoader.ObstacleConverter.Convert))]
        private static IEnumerable<CodeInstruction> ObstacleConvertV3(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.ReplaceCtor(_version3, _obstacleDataCtor, _customObstacleDataCtor);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(
            typeof(BeatmapDataLoaderVersion3.BeatmapDataLoader.SliderConverter),
            nameof(BeatmapDataLoaderVersion3.BeatmapDataLoader.SliderConverter.Convert))]
        private static IEnumerable<CodeInstruction> SliderConvertV3(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.ReplaceMethod(_version3, _createSliderData, _createCustomSliderData);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(
            typeof(BeatmapDataLoaderVersion3.BeatmapDataLoader.BurstSliderConverter),
            nameof(BeatmapDataLoaderVersion3.BeatmapDataLoader.BurstSliderConverter.Convert))]
        private static IEnumerable<CodeInstruction> BurstSliderConverterV3(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.ReplaceMethod(_version3, _createBurstSliderData, _createCustomBurstSliderData);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(
            typeof(BeatmapDataLoaderVersion3.BeatmapDataLoader.WaypointConverter),
            nameof(BeatmapDataLoaderVersion3.BeatmapDataLoader.WaypointConverter.Convert))]
        private static IEnumerable<CodeInstruction> WaypointConvertV3(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.ReplaceCtor(_version3, _waypointCtor, _customWaypointCtor);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(
            typeof(BeatmapDataLoaderVersion3.BeatmapDataLoader.BpmEventConverter),
            nameof(BeatmapDataLoaderVersion3.BeatmapDataLoader.BpmEventConverter.Convert))]
        private static IEnumerable<CodeInstruction> BpmEventConvertV3(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.ReplaceCtor(_version3, _bpmEventCtor, _customBpmEventCtor);
        }

#if PRE_V1_39_1
        [HarmonyTranspiler]
        [HarmonyPatch(
            typeof(BeatmapDataLoaderVersion3.BeatmapDataLoader.RotationEventConverter),
            nameof(BeatmapDataLoaderVersion3.BeatmapDataLoader.RotationEventConverter.Convert))]
        private static IEnumerable<CodeInstruction> RotationEventConvertV3(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.ReplaceCtor(_version3, _rotationEventCtor, _customRotationEventCtor);
        }
#endif

        [HarmonyTranspiler]
        [HarmonyPatch(
            typeof(BeatmapDataLoaderVersion3.BeatmapDataLoader.BasicEventConverter),
            nameof(BeatmapDataLoaderVersion3.BeatmapDataLoader.BasicEventConverter.Convert))]
        private static IEnumerable<CodeInstruction> BasicEventConvertV3(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.ReplaceCtor(_version3, _basicEventCtor, _customBasicEventCtor);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(
            typeof(BeatmapDataLoaderVersion3.BeatmapDataLoader.ColorBoostEventConverter),
            nameof(BeatmapDataLoaderVersion3.BeatmapDataLoader.ColorBoostEventConverter.Convert))]
        private static IEnumerable<CodeInstruction> ColorBoostEventConvertV3(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.ReplaceCtor(_version3, _colorBoostEventCtor, _customColorBoostEventCtor);
        }

        // VERSION 2_6_0AndEarlier
        [HarmonyTranspiler]
        [HarmonyPatch(
            typeof(BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.ColorNoteConverter),
            nameof(BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.ColorNoteConverter.Convert))]
        private static IEnumerable<CodeInstruction> ColorNoteConvertV2_6_0AndEarlier(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Call, _createBasicNoteData))
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Call, _getData),
                    new CodeInstruction(OpCodes.Ldsfld, _version2))
                .SetOperandAndAdvance(_createCustomBasicNoteData)
                .MatchForward(false, new CodeMatch(OpCodes.Call, _createBombNoteData))
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Call, _getData),
                    new CodeInstruction(OpCodes.Ldsfld, _version2))
                .SetOperandAndAdvance(_createCustomBombNoteData)
                .InstructionEnumeration();
        }

        // the transpiler dark magic strikes again...
        // this time our transpiler here causes patches on GetHeightForObstacleType to fail
        // specifically the height patch that mapping extensions needs
        // how a transpiler breaks the patch on a different method is beyond me!!!
        // transpiler giveth and transpiler taketh away
        // for this reason, we'll just replace this with a prefix and override the original method
        /*[HarmonyTranspiler]
        [HarmonyPatch(
            typeof(BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.ObstacleConverter),
            nameof(BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.ObstacleConverter.Convert))]
        private static IEnumerable<CodeInstruction> ObstacleConvertV2_6_0AndEarlier(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.ReplaceCtor(_version2, _obstacleDataCtor, _customObstacleDataCtor);
        }*/

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.ObstacleConverter),
            nameof(BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.ObstacleConverter.Convert))]
        private static bool ObstacleConvertV2_6_0AndEarlier(
            BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.ObstacleConverter __instance,
            BeatmapSaveDataVersion2_6_0AndEarlier.ObstacleData o,
            ref ObstacleData? __result)
        {
            float time = __instance.BeatToTime(o.time);
            float endBeat = o.time + o.duration;
            float duration = __instance.BeatToTime(endBeat) - time;

#if !PRE_V1_39_1
            // https://github.com/Kylemc1413/SongCore/blob/master/source/SongCore/Patches/AllowNegativeObstacleSizeAndDurationPatch.cs
            // match songcore patch by commenting this out
            /*if (o.width < 0 || duration < Mathf.Epsilon)
            {
                __result = null;
                return false;
            }*/
#endif

            __result = new CustomObstacleData(
                time,
#if !PRE_V1_39_1
                o.time,
                endBeat,
                __instance.BeatToRotation(o.time),
#endif
                o.lineIndex,
                BeatmapTypeConverters.ConvertNoteLineLayer(
                    BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.ObstacleConverter
                        .GetLayerForObstacleType(o.type)),
                duration,
                o.width,
                BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.ObstacleConverter
                    .GetHeightForObstacleType(o.type),
                o.GetData(),
                BeatmapSaveDataHelpers.version2);
            return false;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(
            typeof(BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.SliderConverter),
            nameof(BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.SliderConverter.Convert))]
        private static IEnumerable<CodeInstruction> SliderConvertV2_6_0AndEarlier(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.ReplaceMethod(_version2, _createSliderData, _createCustomSliderData);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(
            typeof(BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.WaypointConverter),
            nameof(BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.WaypointConverter.Convert))]
        private static IEnumerable<CodeInstruction> WaypointConvertV2_6_0AndEarlier(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.ReplaceCtor(_version2, _waypointCtor, _customWaypointCtor);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(
            typeof(BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.BasicEventConverter),
            nameof(BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.BasicEventConverter.Convert))]
        private static IEnumerable<CodeInstruction> BasicEventConvertV2_6_0AndEarlier(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                /*.MatchForward(false, new CodeMatch(OpCodes.Newobj, _rotationEventCtor))
                .Repeat(n => n
                    .InsertAndAdvance(
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Call, _getData),
                        new CodeInstruction(OpCodes.Ldsfld, _version2))
                    .SetOperandAndAdvance(_customRotationEventCtor))*/
                .Start()
                .MatchForward(false, new CodeMatch(OpCodes.Newobj, _colorBoostEventCtor))
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Call, _getData),
                    new CodeInstruction(OpCodes.Ldsfld, _version2))
                .SetOperandAndAdvance(_customColorBoostEventCtor)
                .MatchForward(false, new CodeMatch(OpCodes.Newobj, _basicEventCtor))
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Call, _getData),
                    new CodeInstruction(OpCodes.Ldsfld, _version2))
                .SetOperandAndAdvance(_customBasicEventCtor)
                .InstructionEnumeration();
        }
    }
}
#endif
