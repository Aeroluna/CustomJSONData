#if PRE_V1_37_1
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CustomJSONData.CustomBeatmap;
using CustomJSONData.CustomBeatmap.BaseData;
using HarmonyLib;

namespace CustomJSONData.HarmonyPatches
{
    // These converters are static so we cant swap them with our own inherited instances like the others
    [HarmonyPatch(typeof(BeatmapDataLoader.LightColoBaseDataConvertor))]
    public static class BaseData1_34_2AndEarlierCustomify
    {
        private static readonly ConstructorInfo _lightColorBaseDataCtor = AccessTools.FirstConstructor(typeof(LightColorBaseData), _ => true);
        private static readonly ConstructorInfo _customLightColorBaseDataCtor = AccessTools.FirstConstructor(typeof(CustomLightColorBaseData), _ => true);

        private static readonly MethodInfo _getData = AccessTools.Method(typeof(BaseData1_34_2AndEarlierCustomify), nameof(GetData));
        private static readonly MethodInfo _version3 = AccessTools.PropertyGetter(typeof(VersionExtensions), nameof(VersionExtensions.version3));

        private static CustomData GetData(this BeatmapSaveDataVersion3.BeatmapSaveData.LightColorBaseData dataItem)
        {
            return dataItem is ICustomData customData
                ? customData.customData : new CustomData();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(BeatmapDataLoader.LightColoBaseDataConvertor.Convert))]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Newobj, _lightColorBaseDataCtor))
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, _getData),
                    new CodeInstruction(OpCodes.Call, _version3))
                .SetOperandAndAdvance(_customLightColorBaseDataCtor)
                .InstructionEnumeration();
        }
    }
}
#endif
