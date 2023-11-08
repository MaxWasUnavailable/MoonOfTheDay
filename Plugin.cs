using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace MoonOfTheDay;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger;

    private Harmony _harmony;
    private bool _isPatched;
    public static Plugin Instance { get; private set; }

    private void Awake()
    {
        // Set instance
        Instance = this;

        // Init logger
        Logger = base.Logger;

        // Patch using Harmony
        PatchAll();

        // Report plugin is loaded
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
    }

    public void PatchAll()
    {
        if (_isPatched)
        {
            Logger.LogWarning("Already patched!");
            return;
        }

        Logger.LogDebug("Patching...");

        _harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        _harmony.PatchAll();
        _isPatched = true;

        Logger.LogDebug("Patched!");
    }

    public void UnpatchAll()
    {
        if (!_isPatched)
        {
            Logger.LogWarning("Not patched!");
            return;
        }

        Logger.LogDebug("Unpatching...");

        _harmony.UnpatchSelf();
        _isPatched = false;

        Logger.LogDebug("Unpatched!");
    }
}