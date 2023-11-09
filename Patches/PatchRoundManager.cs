using HarmonyLib;
using UnityEngine;
using UnityEngine.AI;
using Random = System.Random;

namespace MoonOfTheDay.Patches;

/// <summary>
///     Helper methods for the RoundManager patch
/// </summary>
public static class PatchRoundManagerHelpers
{
    public static Vector3 SeededInsideUnitSphere(ref Random random)
    {
        return new Vector3((float)(random.NextDouble() * 2 - 1),
            (float)(random.NextDouble() * 2 - 1),
            (float)(random.NextDouble() * 2 - 1));
    }
}

/// <summary>
///     Patches for the RoundManager class
///
///     Makes both GetRandomNavMeshPositionInRadius and GetRandomNavMeshPositionInRadiusSpherical use a deterministic
///     random number generator from the RoundManager instance instead of Unity's default random number generator.
///
///     This allows for the seed to impact the spawn location of loot and enemies.
/// </summary>
[HarmonyPatch(typeof(RoundManager))]
public class PatchRoundManager
{
    [HarmonyPatch("GetRandomNavMeshPositionInRadiusSpherical")]
    [HarmonyPrefix]
    public static bool GetRandomNavMeshPositionInRadiusSpherical(RoundManager __instance, ref Vector3 __result,
        Vector3 pos, float radius = 10f,
        NavMeshHit navHit = default)
    {
        pos = PatchRoundManagerHelpers.SeededInsideUnitSphere(ref __instance.LevelRandom) * radius + pos;
        __result = NavMesh.SamplePosition(pos, out navHit, radius, -1) ? navHit.position : pos;
        return false;
    }

    [HarmonyPatch("GetRandomNavMeshPositionInRadius")]
    [HarmonyPrefix]
    public static bool GetRandomNavMeshPositionInRadius(RoundManager __instance, ref Vector3 __result, Vector3 pos,
        float radius = 10f,
        NavMeshHit navHit = default)
    {
        var y = pos.y;
        pos = PatchRoundManagerHelpers.SeededInsideUnitSphere(ref __instance.LevelRandom) * radius + pos;
        pos.y = y;
        if (NavMesh.SamplePosition(pos, out navHit, radius, -1))
        {
            __result = navHit.position;
            return false;
        }

        Debug.Log("Unable to get random nav mesh position in radius! Returning old pos");
        __result = pos;
        return false;
    }
}