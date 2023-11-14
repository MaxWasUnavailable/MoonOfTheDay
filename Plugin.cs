using System;
using System.Collections.Generic;
using System.Linq;
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
    private static int HighestLevelID { get; set; } = -1;
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

    private static void SetHighestLevelID(IEnumerable<SelectableLevel> moons)
    {
        if (HighestLevelID != -1) return;

        HighestLevelID = moons.Max(moon => moon.levelID);

        Logger.LogInfo($"Highest vanilla level ID is {HighestLevelID}");
    }

    private static SelectableLevel GetCustomMoon(SelectableLevel[] moons, bool isDailyElseWeekly = true)
    {
        SetHighestLevelID(moons);

        var random = new Random(isDailyElseWeekly ? GetDailySeed() : GetWeeklySeed());

        var moonsWithoutCompany = moons.Where(moon => moon.levelID != 3).ToArray();

        var moon =
            Instantiate(
                moonsWithoutCompany.OrderBy(moon => moon.levelID).ToArray()[
                    random.Next(0, moonsWithoutCompany.Length)]);

        moon.name = isDailyElseWeekly ? DailyMoonName : WeeklyMoonName;
        moon.PlanetName = isDailyElseWeekly ? DailyMoonName : WeeklyMoonName;
        moon.LevelDescription = "This moon looks familiar...";
        moon.riskLevel = "???";
        moon.levelID = HighestLevelID + (isDailyElseWeekly ? 1 : 2);

        // TODO: investigate RandomWeatherWithVariables --> variables might prevent complete flooding on some moons?

        moon.randomWeathers = Array.Empty<RandomWeatherWithVariables>();
        moon.overrideWeather = true;

        var weatherOverride = Enum.GetValues(typeof(LevelWeatherType)).Cast<LevelWeatherType>().ToArray()[
            random.Next(0, Enum.GetValues(typeof(LevelWeatherType)).Length)];

        moon.overrideWeatherType = weatherOverride;
        moon.currentWeather = weatherOverride;

        return moon;
    }

    public static SelectableLevel GetDailyMoon(SelectableLevel[] moons)
    {
        return GetCustomMoon(moons);
    }

    public static SelectableLevel GetWeeklyMoon(SelectableLevel[] moons)
    {
        return GetCustomMoon(moons, false);
    }
}