using System.Linq;
using HarmonyLib;

namespace MoonOfTheDay.Patches;

public static class PatchStartOfRoundHelpers
{
    public static bool IsVanillaMoon(SelectableLevel moon)
    {
        return !new [] {Plugin.DailyMoonName, Plugin.WeeklyMoonName}.Contains(moon.PlanetName);
    }
}

[HarmonyPatch(typeof(StartOfRound))]
public class PatchStartOfRound
{
    [HarmonyPatch("StartGame")]
    [HarmonyPrefix]
    public static void SetSeed(StartOfRound __instance)
    {
        if (__instance.currentLevel == null) return;
        if (PatchStartOfRoundHelpers.IsVanillaMoon(__instance.currentLevel)) return;
        
        Plugin.Logger.LogInfo($"Setting seed for {__instance.currentLevel.PlanetName}...");
        
        __instance.overrideRandomSeed = true;
        
        switch (__instance.currentLevel.PlanetName)
        {
            case Plugin.DailyMoonName:
                __instance.overrideSeedNumber = Plugin.GetDailySeed();
                break;
            case Plugin.WeeklyMoonName:
                __instance.overrideSeedNumber = Plugin.GetWeeklySeed();
                break;
        }
    }
    
    [HarmonyPatch("Awake")]
    [HarmonyPostfix]
    public static void InsertMoons(StartOfRound __instance)
    {
        Plugin.Logger.LogInfo("Inserting moons in StartOfRound...");

        var startOfRound = StartOfRound.Instance;
        
        var levelList = startOfRound.levels.ToList();
        
        var dailyMoon = Plugin.GetDailyMoon(startOfRound.levels);
        var weeklyMoon = Plugin.GetWeeklyMoon(startOfRound.levels);

        // Remove first to prevent potential issues with duplicate moons
        levelList.RemoveAll(level => level.PlanetName == dailyMoon.PlanetName);
        levelList.RemoveAll(level => level.PlanetName == weeklyMoon.PlanetName);

        levelList.Add(dailyMoon);
        levelList.Add(weeklyMoon);

        startOfRound.levels = levelList.ToArray();
    }
}