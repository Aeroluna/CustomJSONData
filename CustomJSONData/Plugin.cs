using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace CustomJSONData
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("BSIPA_Utilities")]
    [BepInProcess("Beat Saber.exe")]
    internal class Plugin : BaseUnityPlugin
    {
        private readonly Harmony _harmonyInstance = new("dev.aeroluna.CustomJSONData");

        internal static ManualLogSource Log { get; set; } = null!;

        private void Awake()
        {
            Log = Logger;
        }

        private void Start()
        {
            _harmonyInstance.PatchAll(typeof(Plugin).Assembly);
        }

        private void OnDestroy()
        {
            _harmonyInstance.UnpatchSelf();
        }
    }
}
