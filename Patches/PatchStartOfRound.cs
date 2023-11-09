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
}