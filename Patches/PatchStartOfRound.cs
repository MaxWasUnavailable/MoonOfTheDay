using HarmonyLib;

namespace MoonOfTheDay.Patches;

[HarmonyPatch(typeof(StartOfRound))]
public class PatchStartOfRound
{
    [HarmonyPatch("StartGame")]
    [HarmonyPrefix]
    public static void SetSeed(StartOfRound __instance)
    {
        __instance.overrideRandomSeed = true;
        __instance.overrideSeedNumber = 123456789;
    }
}