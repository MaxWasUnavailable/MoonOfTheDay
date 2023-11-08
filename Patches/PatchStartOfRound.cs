﻿using HarmonyLib;

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
    
    [HarmonyPatch("StartGame")]
    [HarmonyPostfix]
    public static void SetScreenLevelDescription(StartOfRound __instance)
    {
        __instance.screenLevelDescription.text = "Moon of the Day";
    }
}