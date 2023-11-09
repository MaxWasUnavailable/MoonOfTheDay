using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace MoonOfTheDay;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public const string DailyMoonName = "Daily Moon";
    public const string WeeklyMoonName = "Weekly Moon";
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

    /// <summary>
    ///     Generate a seed based on the day.
    /// </summary>
    /// <returns> Seed </returns>
    public static int GetDailySeed()
    {
        return (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalDays;
    }

    /// <summary>
    ///     Generate a seed based on the week.
    /// </summary>
    /// <returns> Seed </returns>
    public static int GetWeeklySeed()
    {
        var day = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalDays;
        return (int)Math.Floor((float)day / 7);
    }

    public static SelectableLevel GetDailyMoon(SelectableLevel[] moons)
    {
        var random = new Random(GetDailySeed());

        var dailyMoon = moons[random.Next(0, moons.Length)];

        // Copy the moon to prevent reference issues
        dailyMoon = Instantiate(dailyMoon);

        dailyMoon.PlanetName = DailyMoonName;
        dailyMoon.LevelDescription = "This moon looks familiar...";
        dailyMoon.riskLevel = "???";
        dailyMoon.levelID = 999;

        return dailyMoon;
    }

    public static SelectableLevel GetWeeklyMoon(SelectableLevel[] moons)
    {
        var random = new Random(GetWeeklySeed());

        var weeklyMoon = moons[random.Next(0, moons.Length)];

        // Copy the moon to prevent reference issues
        weeklyMoon = Instantiate(weeklyMoon);

        weeklyMoon.PlanetName = WeeklyMoonName;
        weeklyMoon.LevelDescription = "This moon looks familiar...";
        weeklyMoon.riskLevel = "???";
        weeklyMoon.levelID = 998;

        return weeklyMoon;
    }
}