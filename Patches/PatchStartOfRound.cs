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
        if (Plugin.Instance.SelectedMoonType == MoonType.Normal) return;
        
        __instance.overrideRandomSeed = true;
        
        switch (Plugin.Instance.SelectedMoonType)
        {
            case MoonType.Daily:
                __instance.overrideSeedNumber = Plugin.GetDailySeed();
                break;
            case MoonType.Weekly:
                __instance.overrideSeedNumber = Plugin.GetWeeklySeed();
                break;
            default:
                Plugin.Logger.LogWarning($"Unknown moon type: {Plugin.Instance.SelectedMoonType}");
                return;
        }
    }
    
    [HarmonyPatch("SetMapScreenInfoToCurrentLevel")]
    [HarmonyPostfix]
    public static void SetScreenLevelDescription(StartOfRound __instance)
    {
        if (Plugin.Instance.SelectedMoonType == MoonType.Normal) return;

        string text;

        switch (Plugin.Instance.SelectedMoonType)
        {
            case MoonType.Daily:
                text = "Daily Moon";
                break;
            case MoonType.Weekly:
                text = "Weekly Moon";
                break;
            default:
                Plugin.Logger.LogWarning($"Unknown moon type: {Plugin.Instance.SelectedMoonType}");
                return;
        }
        
        __instance.screenLevelDescription.text = text;
    }
}